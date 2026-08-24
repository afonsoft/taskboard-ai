# Plugins e Integrações

## Jira Sync

Pull e push de issues entre o Taskboard e um projeto Jira Cloud (`jira-my-tasks`).

- Endpoint: `/api/local/jira-connection`
- Auth: Basic (email + token)
- Resolução de conflitos: otimista (`version` + 409)

## DeepSeek Harness

Adaptador para o ecossistema DeepSeek consumir o CLI `taskctl` e o servidor MCP.

## Cloudflare Proxy / Companion

- Companion loopback local
- Proxy Cloudflare D1 (banco) e R2 (anexos)
- Basic Auth para companion
- Intervalo de polling de revisão: 2000ms

## MCP Server

13 tools mapeadas a partir de comandos REST API/CLI. Transporte: stdio ou HTTP/SSE.

## AI Chat

Camada abstraída de provider para OpenAI, Claude ou Azure OpenAI.
- Threads, runs, events
- Catalog de modelos

Veja as specs individuais em `.specs/` para detalhes.
