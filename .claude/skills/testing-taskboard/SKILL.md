---
name: testing-taskboard
description: End-to-end testing guide for the taskboard-ai Blazor Server app, install.sh, and CLI/MCP wrappers.
tools:
  - Bash
  - Read
  - computer
---

## Context

This skill records how to run an isolated, clean end-to-end test of `taskboard-ai` on a fresh session.

## One-time setup

- .NET 10 SDK must be on `PATH`.
- `google-chrome` must be installed for UI tests.

## Isolated install

```bash
export TEST_HOME=$(mktemp -d)
export HOME=$TEST_HOME
export TASKBOARD_HOME=$TEST_HOME/.taskboard
cd /home/ubuntu/repos/taskboard-ai
./install.sh
```

This builds the solution, installs the `taskctl` dotnet tool into `$TASKBOARD_HOME/bin`, and creates the `taskboard-server` and `taskboard-mcp` wrappers plus `$TASKBOARD_HOME/env`.

## Start the server

```bash
source "$TASKBOARD_HOME/env"
taskboard-server > "$TEST_HOME/server.log" 2>&1 &
echo $! > "$TEST_HOME/server.pid"
```

Wait for `http://127.0.0.1:47823/health` to return `200`.

## Admin credentials

`install.sh` writes the generated admin password to `$TASKBOARD_HOME/admin-password`. The username is always `admin`.

## Quick verification commands

```bash
curl -sf http://127.0.0.1:47823/health
curl -sI http://127.0.0.1:47823/  # expect 302 Location: /login
taskctl project list --json       # expect at least the default "local" project
cat "$TEST_HOME/admin-password"   # use this for the browser login form
```

## MCP stdio smoke test

Send line-delimited JSON-RPC to `$TASKBOARD_HOME/bin/taskboard-mcp`:

```json
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0"}}}
{"jsonrpc":"2.0","method":"notifications/initialized"}
{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"list_projects","arguments":{}}}
```

The `initialize` response should contain `result.protocolVersion`; the `tools/call` response should list a project with `id` `local`.

## Browser test notes

- Open `http://127.0.0.1:47823/login`.
- Use username `admin` and the generated password.
- After login the browser should be at `/` and the board should show the columns `backlog`, `todo`, `in_progress`, `in_review`, `blocked`, `done`, `canceled`, and `archived`.
- Clicking the `Log out` button should return to `/login`.

## Cleanup

```bash
kill $(cat "$TEST_HOME/server.pid")
rm -rf "$TEST_HOME"
```

## Common issues

- If `install.sh` cannot write `$TASKBOARD_HOME/admin-password`, make sure `TASKBOARD_HOME` is exported before running it.
- If the login form post 302s back to `/login`, the admin password in `$TASKBOARD_HOME/admin-password` may not match the server env; restart the server after sourcing the env file.
- The CORS `Dev` policy only allows `http://localhost:5173`; same-origin browser requests to `127.0.0.1:47823` will still work but may log CORS warnings.
