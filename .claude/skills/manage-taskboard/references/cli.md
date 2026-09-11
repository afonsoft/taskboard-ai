# Referência do CLI `taskctl`

O `taskctl` é a interface de linha de comando .NET do Taskboard. Todos os comandos suportam `--json` para saída processável por máquina e `--url <URL>` para sobrescrever o endpoint da API.

## Opções globais

```
--url     URL base da API REST do Taskboard
--json    Retorna resultados como JSON
```

## project

```bash
# Listar projetos
taskctl project list [--json]

# Obter projeto
taskctl project get --id <project-id> [--json]

# Criar projeto
taskctl project create --name <nome> [--id <id>] [--workspace <caminho>] [--json]
```

## issue

```bash
# Listar issues
taskctl issue list --project <projeto> [--status <status>] [--json]

# Obter issue
taskctl issue get --identifier TASK-<projeto>-<N> [--json]

# Criar issue
taskctl issue create --project <projeto> --title <título> \
  [--description <texto>] [--status todo] [--priority medium] \
  [--label <rótulo>] [--start-date <yyyy-MM-dd>] [--due-date <yyyy-MM-dd>] \
  [--json]

# Atualizar issue
taskctl issue update --identifier TASK-<projeto>-<N> \
  [--title <título>] [--description <texto>] [--status <status>] \
  [--priority <priority>] [--version <version>] [--json]

# Mover issue
taskctl issue move --identifier TASK-<projeto>-<N> --status <status> \
  [--sort-order <número>] [--version <version>] [--json]

# Arquivar/restaurar issue
taskctl issue archive --identifier TASK-<projeto>-<N> [--version <version>] [--json]
taskctl issue restore --identifier TASK-<projeto>-<N> [--version <version>] [--json]
```

## comment

```bash
# Listar comentários
taskctl comment list --identifier TASK-<projeto>-<N> [--json]

# Criar comentário
taskctl comment create --identifier TASK-<projeto>-<N> --body <texto> [--json]
```

## attachment

```bash
# Listar anexos
taskctl attachment list --identifier TASK-<projeto>-<N> [--json]

# Enviar anexo
taskctl attachment upload --identifier TASK-<projeto>-<N> --file <caminho> [--json]
```

## cloud

```bash
# Status da sessão cloud
taskctl cloud status [--json]

# Configurar companion cloud
taskctl cloud configure --companion-url <url> --username <user> --password <senha> --project <id-projeto>
```

## context

```bash
# Exibir contexto atual
taskctl context current [--json]

# Definir projeto do contexto
taskctl context set-project --project <projeto>

# Definir workspace do contexto
taskctl context set-workspace --workspace <caminho>
```

## Códigos de saída

| Código | Significado |
|---|---|
| 0 | Sucesso |
| 1 | Erro genérico |
| 2 | Erro de validação |
| 3 | Servidor offline |
| 4 | Erro de autenticação |
| 5 | Conflito (version mismatch) |

## Arquivo de configuração

O `taskctl` armazena configurações em `~/.config/taskctl/settings.json` (macOS: `~/Library/Application Support/taskctl/settings.json`).
