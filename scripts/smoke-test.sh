#!/usr/bin/env bash
# ---------------------------------------------------------------
#  Testes de fumaça pós-deploy: valida a API rodando de verdade
#  (com banco Oracle) no ambiente informado.
#  Uso: ./scripts/smoke-test.sh http://localhost:8081
# ---------------------------------------------------------------
set -euo pipefail

BASE_URL="${1:-http://localhost:8080}"
PASS=0

check() { echo "  [OK] $1"; PASS=$((PASS + 1)); }

echo "==> Smoke tests em $BASE_URL"

# 1) Health check (mostra ambiente e status do banco)
HEALTH=$(curl -fsS "$BASE_URL/health")
echo "  /health -> $HEALTH"
echo "$HEALTH" | jq -e '.status == "Healthy" and .database == "up"' >/dev/null
check "Health check: API e banco no ar ($(echo "$HEALTH" | jq -r .environment))"

# 2) Swagger
curl -fsS -o /dev/null "$BASE_URL/swagger/v1/swagger.json"
check "Swagger publicado"

# 3) Login (JWT)
TOKEN=$(curl -fsS -X POST "$BASE_URL/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}' | jq -r .accessToken)
[[ -n "$TOKEN" && "$TOKEN" != "null" ]]
check "Login admin retornou token JWT"

# 4) Listagem paginada de pontos de coleta
POINTS=$(curl -fsS "$BASE_URL/api/CollectionPoints?pageNumber=1&pageSize=5")
TOTAL=$(echo "$POINTS" | jq -r .totalItems)
POINT_ID=$(echo "$POINTS" | jq -r '.items[0].id')
check "GET /api/CollectionPoints ($TOTAL pontos)"

# 5) Endpoint protegido sem token deve retornar 401
CODE=$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE_URL/api/WasteRecords" \
  -H "Content-Type: application/json" -d '{}')
[[ "$CODE" == "401" ]]
check "POST sem token bloqueado (401)"

# 6) Registro de resíduo com token (regra de negócio ESG)
RESULT=$(curl -fsS -X POST "$BASE_URL/api/WasteRecords" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d "{\"collectionPointId\":$POINT_ID,\"type\":2,\"weightKg\":12.5,\"description\":\"smoke test pipeline\"}")
echo "  registro -> $(echo "$RESULT" | jq -c '{id: .record.id, ocupacao: .occupancyPercent, alerta: .alertTriggered}')"
check "POST /api/WasteRecords registrou descarte"

# 7) Relatório e alertas
curl -fsS -o /dev/null "$BASE_URL/api/WasteRecords/report"
check "GET /api/WasteRecords/report"
curl -fsS -o /dev/null "$BASE_URL/api/Alerts"
check "GET /api/Alerts"

echo "==> $PASS verificações passaram em $BASE_URL"
