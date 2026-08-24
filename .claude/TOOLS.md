# TOOLS.md — Ferramentas e MCP

## Ferramentas Nativas

| Tool | Uso | Política |
|---|---|---|
| Read | Ler specs, código, docs | Livre |
| Edit | Modificar arquivos | Confirmar se multi-arquivo |
| Write | Criar arquivos | Confirmar se fora do escopo |
| Grep | Buscar símbolos | Livre |
| Glob | Listar arquivos | Livre |
| Bash | Build, test, git | Sandbox + logging |
| Mcp | Integrações externas | Rate-limited |

## MCP Servers

| Nome | Transporte | Uso |
|---|---|---|
| taskboard-local | stdio / HTTP | Ferramentas do próprio taskboard (futuro) |

## APIs Externas

| API | Headers | Timeout | Rate Limit |
|---|---|---|---|
| OpenAI/Claude/Azure | `Authorization: Bearer $API_KEY` | 60s | conforme provider |
| Jira REST | `Authorization: Basic $TOKEN` | 30s | 10 req/s |
| Cloudflare D1/R2 | `Authorization: Bearer $CF_TOKEN` | 30s | conforme plano |

## Princípios de Design de Tools

- Nomear pelo que fazem, não como fazem.
- Schemas mínimos.
- Erros em JSON.
- Operações idempotentes.
