#!/usr/bin/env bash
# ---------------------------------------------------------------
#  Deploy do EcoWaste com Docker Compose em um ambiente.
#  Uso:  ./scripts/deploy.sh <staging|production> [imagem]
#  Ex.:  ./scripts/deploy.sh staging ghcr.io/usuario/ecowaste-api:abc123
#
#  Segredos (ORACLE_PASSWORD, DB_PASSWORD, JWT_SECRET) são lidos do
#  ambiente do shell. Se não existirem, são gerados aleatoriamente.
# ---------------------------------------------------------------
set -euo pipefail

ENVIRONMENT="${1:?informe o ambiente: staging ou production}"
IMAGE="${2:-ecowaste-api:local}"
ENV_FILE="env/${ENVIRONMENT}.env"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Arquivo de ambiente não encontrado: $ENV_FILE" >&2
  exit 1
fi

# Carrega variáveis não sensíveis do ambiente escolhido
set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

export API_IMAGE="$IMAGE"
export APP_VERSION="${APP_VERSION:-$(git rev-parse --short HEAD 2>/dev/null || echo local)}"
export ORACLE_PASSWORD="${ORACLE_PASSWORD:-$(openssl rand -hex 16)}"
export DB_PASSWORD="${DB_PASSWORD:-$(openssl rand -hex 16)}"
export JWT_SECRET="${JWT_SECRET:-$(openssl rand -hex 32)}"

COMPOSE=(docker compose --env-file "$ENV_FILE" -p "$COMPOSE_PROJECT_NAME")

echo "==> Deploy no ambiente: $ENVIRONMENT"
echo "    Projeto : $COMPOSE_PROJECT_NAME"
echo "    Imagem  : $API_IMAGE"
echo "    Versão  : $APP_VERSION"
echo "    Porta   : $API_PORT"

# Usa imagem já publicada (pipeline) ou faz build local
if [[ "$IMAGE" == "ecowaste-api:local" ]]; then
  "${COMPOSE[@]}" up -d --build
else
  "${COMPOSE[@]}" pull api || true
  "${COMPOSE[@]}" up -d --no-build
fi

echo "==> Aguardando a API ficar saudável (o Oracle pode levar ~1-2 min)..."
for i in $(seq 1 60); do
  if curl -fsS "http://localhost:${API_PORT}/health" >/dev/null 2>&1; then
    echo "==> API saudável após $((i * 5))s"
    "${COMPOSE[@]}" ps
    exit 0
  fi
  sleep 5
done

echo "!! A API não respondeu a tempo. Logs:" >&2
"${COMPOSE[@]}" logs --tail=200 >&2
exit 1
