# SPEC-005: CLI taskctl

## 0. SPEC Metadata

| Field | Value |
|---|---|
| Feature name | CLI taskctl |
| Product / System | dashi-taskboard |
| Module / Bounded Context | CLI |
| Change type | Migration |
| Repository | afonsoft/dashi-taskboard |
| Suggested branch | devin/spec-cli-net10 |
| Technical owner | afonsoft |
| Status | Draft |
| Date | 2026-08-24 |
| Target agent | Devin |

---

## 1. Executive Summary

### Problem

O CLI atual `cli/taskctl.mjs` oferece comandos para projetos, cloud, issues, comentários, anexos e contexto.

### Objective

Criar um CLI .NET global tool `taskctl` (usando `System.CommandLine`) que consuma a API REST do Taskboard e ofereça os mesmos subcomandos e saída JSON.

### Expected outcome

`taskctl` em .NET 10 equivalente ao Node.js: `project list/create/map`, `cloud login/status/logout`, `issue list/get/create/update/move/archive/restore/relation`, `comment list/add/update/delete`, `attachment download/upload`, `context current`.

### Out of scope

- Empacotamento nuget (pode ser feito posteriormente).

---

## 2. Agent Role

> Senior .NET CLI engineer usando System.CommandLine e HttpClient.

---

## 3. Agent Autonomy Level

3

---

## 4. Product Context

### Functional context

O CLI permite automação, scripts e integração com agentes de IA fora do navegador.

### Technical context

- Node.js `cli/taskctl.mjs`.
- Default API URL `http://127.0.0.1:47823`.
- Saída JSON opcional (`--json`).
- Schema version 2.

### Relevant stack

- .NET 10
- `System.CommandLine` (ou `CommandLineParser`)
- `HttpClient`
- `Microsoft.Extensions.DependencyInjection`

---

## 5. Task Definition

### Main task

Implementar CLI `taskctl` em C#.

### Subtasks

- `project list`, `project create`, `project map`
- `cloud login`, `cloud status`, `cloud logout`
- `issue list`, `issue get`, `issue create`, `issue update`, `issue move`, `issue archive`, `issue restore`, `issue relation`
- `comment list`, `comment add`, `comment update`, `comment delete`
- `attachment download`, `attachment upload`
- `context current`

### Do not do

- Não implementar autenticação complexa de cloud nesta spec (ver SPEC-010).

---

## 6. Functional Requirements

### FR-001: Configuração

**Description:**  
Ler URL base de `TASKBOARD_URL` ou `--url`; armazenar config em `~/.config/taskctl/settings.json`.

### FR-002: Projetos

```bash
taskctl project list --json
taskctl project create --id my-project --name "My Project" --workspace-path /path --json
taskctl project map my-project --workspace-path /path
```

### FR-003: Issues

```bash
taskctl issue list --project local --status todo --json
taskctl issue get TASK-local-1 --json
taskctl issue create --project local --title "x" --status todo --priority high --json
taskctl issue update TASK-local-1 --title "y" --json
taskctl issue move TASK-local-1 --status done --json
taskctl issue archive TASK-local-1 --json
taskctl issue restore TASK-local-1 --json
taskctl issue relation TASK-local-1 add parent TASK-local-2
taskctl issue relation TASK-local-1 remove parent TASK-local-2
```

### FR-004: Comentários

```bash
taskctl comment list TASK-local-1 --json
taskctl comment add TASK-local-1 "texto" --json
taskctl comment update COMMENT-ID "novo" --json
taskctl comment delete COMMENT-ID
```

### FR-005: Anexos

```bash
taskctl attachment upload TASK-local-1 /path/to/file.png --json
taskctl attachment download ATTACHMENT-ID /output/path.png
```

### FR-006: Contexto

```bash
taskctl context current --json
```

---

## 7. Business Rules

- CLI sempre consome API HTTP; nenhuma lógica de negócio local.
- Saída `--json` é o padrão para integração com agentes.
- Erros da API são propagados com código e mensagem.

---

## 8. Domain Modeling

Nenhum; CLI é presentation.

---

## 9. Expected Architecture

Console app `Taskboard.Cli` usando `System.CommandLine` e `HttpClientFactory`.

---

## 10. API Contracts

Ver SPEC-004.

---

## 11. Application Contracts

Não aplica.

---

## 12. Persistence and Data

Config local em JSON.

---

## 13. Integrations

Taskboard HTTP API.

---

## 14. Edge Cases and Error Scenarios

| Scenario | Input | Expected behavior |
|---|---|---|
| Servidor offline | --url inválido | mensagem de erro amigável |
| Argumentos insuficientes | create sem --title | help |
| Resposta não-JSON | --json com erro | stderr claro |

---

## 15. Few-Shot Examples

```bash
taskctl project list --json
# [{"id":"local","name":"全局","issueCount":0}]
```

---

## 16-24. Standard SSD sections

---

## Pending Questions

1. A config pode usar `appsettings.json` ou `~/.taskctl/config.json`?
2. Empacotar como global tool NuGet?

## Human Approval Checklist

Seguir template padrão SSD.
