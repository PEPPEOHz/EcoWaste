# Evidências do pipeline

Depois de rodar o pipeline na branch `main`, baixe os artifacts na página da execução (aba **Actions** → execução → seção *Artifacts*) e extraia aqui:

- `evidencias-staging/`: deploy-staging.log, smoke-staging.log, health-staging.json, containers-staging.txt, logs da API e do banco
- `evidencias-production/`: os mesmos arquivos para produção
- `test-results/`: relatório dos testes (.trx) e saída do console
