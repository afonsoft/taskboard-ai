# Plugins and Integrations

## Jira Sync

Pull and push issues between Taskboard and a Jira Cloud project (`jira-my-tasks`).

- Endpoint: `/api/local/jira-connection`
- Auth: Basic (email + token)
- Conflict resolution: optimistic concurrency (`version` + 409)

## DeepSeek Harness

Adapter allowing the DeepSeek ecosystem to consume the `taskctl` CLI and MCP server.

## Cloudflare Proxy / Companion

- Cloud companion loopback
- Cloudflare D1 (database) and R2 (attachments) proxy
- Basic Auth for companion
- Review polling interval: 2000ms

## MCP Server

13 tools mapped from REST API/CLI commands. Transport: stdio or HTTP/SSE.

## AI Chat

Abstracted provider layer for OpenAI, Claude, or Azure OpenAI.
- Threads, runs, events
- Per-thread SSE
- Model catalog

See individual specs in `.specs/` for details.
