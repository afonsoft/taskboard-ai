using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Taskboard.Cli.Services;
using Taskboard.Requests;
using Taskboard.ValueObjects;

namespace Taskboard.Cli;

internal static class Program
{
    private static readonly Option<string> UrlOption = new("--url", "URL base da API (default: config ou 127.0.0.1:47823)");
    private static readonly Option<bool> JsonOption = new("--json", "Saída em JSON");

    static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("taskctl - Taskboard CLI");
        root.AddGlobalOption(UrlOption);
        root.AddGlobalOption(JsonOption);

        root.AddCommand(CreateProjectCommand());
        root.AddCommand(CreateIssueCommand());
        root.AddCommand(CreateCommentCommand());
        root.AddCommand(CreateAttachmentCommand());
        root.AddCommand(CreateCloudCommand());
        root.AddCommand(CreateContextCommand());

        try
        {
            return await root.InvokeAsync(args);
        }
        catch (CliException ex)
        {
            await Console.Error.WriteLineAsync($"error: {ex.Message}");
            return ex.ExitCode;
        }
        catch (OperationCanceledException)
        {
            return 130;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"error: {ex.Message}");
            return 1;
        }
    }

    private static string GetBaseUrl(InvocationContext ctx)
    {
        var env = Environment.GetEnvironmentVariable("TASKBOARD_URL");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        var arg = ctx.ParseResult.GetValueForOption(UrlOption);
        if (!string.IsNullOrWhiteSpace(arg))
        {
            return arg;
        }

        return CliConfigService.Load().BaseUrl;
    }

    private static TaskboardApiClient CreateClient(InvocationContext ctx)
        => new(GetBaseUrl(ctx));

    private static bool IsJson(InvocationContext ctx)
        => ctx.ParseResult.GetValueForOption(JsonOption);

    private static async Task<int> WriteOutputAsync(InvocationContext ctx, JsonNode? node, string? extractPath = null)
    {
        if (extractPath is not null && node is not null)
        {
            node = node[extractPath];
        }

        if (IsJson(ctx))
        {
            Console.WriteLine(node?.ToJsonString() ?? "{}");
            return 0;
        }

        if (node is null)
        {
            Console.WriteLine("OK");
            return 0;
        }

        if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                Console.WriteLine(item?.ToJsonString() ?? string.Empty);
            }
        }
        else if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("id", out var id) && id is not null)
            {
                Console.WriteLine($"{id}: {obj["title"] ?? obj["name"] ?? obj}");
            }
            else
            {
                Console.WriteLine(obj.ToJsonString());
            }
        }
        else
        {
            Console.WriteLine(node.ToJsonString());
        }

        return 0;
    }

    private static async Task<string?> ResolveTaskIdAsync(TaskboardApiClient client, string identifier, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        var all = await client.GetAsync("/api/tasks", ct);
        var tasks = all?["tasks"] as JsonArray;
        if (tasks is null)
        {
            return null;
        }

        foreach (var task in tasks)
        {
            if (task?["identifier"]?.GetValue<string>() == identifier || task?["id"]?.GetValue<string>() == identifier)
            {
                return task["id"]?.GetValue<string>();
            }
        }

        return null;
    }

    private static async Task<(string? TaskId, string? CommentId)> ResolveCommentAsync(TaskboardApiClient client, string commentId, CancellationToken ct)
    {
        var all = await client.GetAsync("/api/tasks", ct);
        var tasks = all?["tasks"] as JsonArray;
        if (tasks is null)
        {
            return (null, null);
        }

        foreach (var task in tasks)
        {
            var id = task?["id"]?.GetValue<string>();
            if (id is null)
            {
                continue;
            }

            var comments = await client.GetAsync($"/api/tasks/{id}/comments", ct);
            var list = comments?["comments"] as JsonArray;
            if (list is null)
            {
                continue;
            }

            foreach (var c in list)
            {
                if (c?["id"]?.GetValue<string>() == commentId)
                {
                    return (id, commentId);
                }
            }
        }

        return (null, null);
    }

    private static Command CreateProjectCommand()
    {
        var project = new Command("project", "Gerenciar projetos");

        var list = new Command("list", "Listar projetos");
        list.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var result = await client.GetAsync("/api/projects", ct);
            return await WriteOutputAsync(ctx, result, "projects");
        });

        var idOpt = new Option<string?>("--id", () => null, "ID do projeto");
        var nameOpt = new Option<string>("--name", "Nome do projeto") { IsRequired = true };
        var workspaceOpt = new Option<string?>("--workspace-path", () => null, "Caminho do workspace");

        var create = new Command("create", "Criar projeto")
        {
            idOpt,
            nameOpt,
            workspaceOpt,
        };
        create.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var id = ctx.ParseResult.GetValueForOption(idOpt);
            var name = ctx.ParseResult.GetValueForOption(nameOpt)!;
            var workspace = ctx.ParseResult.GetValueForOption(workspaceOpt);
            var payload = new CreateProjectRequest(id, name, workspace);
            var result = await client.PostAsync("/api/projects", payload, ct);
            return await WriteOutputAsync(ctx, result, "project");
        });

        var mapProjectArg = new Argument<string>("project", "ID do projeto");
        var mapWorkspaceOpt = new Option<string?>("--workspace-path", () => null, "Caminho do workspace");
        var map = new Command("map", "Definir projeto/workspace atual")
        {
            mapProjectArg,
            mapWorkspaceOpt,
        };
        map.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var config = CliConfigService.Load();
            var projectId = ctx.ParseResult.GetValueForArgument(mapProjectArg);
            var workspace = ctx.ParseResult.GetValueForOption(mapWorkspaceOpt);
            config.CurrentProject = projectId;
            config.CurrentWorkspace = workspace ?? config.CurrentWorkspace;
            CliConfigService.Save(config);

            var node = new JsonObject
            {
                ["projectId"] = projectId,
                ["workspacePath"] = workspace,
            };
            return await WriteOutputAsync(ctx, node);
        });

        project.AddCommand(list);
        project.AddCommand(create);
        project.AddCommand(map);
        return project;
    }

    private static Command CreateIssueCommand()
    {
        var issue = new Command("issue", "Gerenciar issues/tarefas");

        var projectOpt = new Option<string?>("--project", () => null, "ID do projeto");
        var statusOpt = new Option<string?>("--status", () => null, "Status");
        var archivedOpt = new Option<bool?>("--archived", () => null, "Incluir arquivadas");
        var assigneeOpt = new Option<string?>("--assignee", () => null, "Assignee");
        var labelOpt = new Option<string?>("--label", () => null, "Label");
        var titleOpt = new Option<string?>("--title", () => null, "Título");
        var descriptionOpt = new Option<string?>("--description", () => null, "Descrição");
        var priorityOpt = new Option<string?>("--priority", () => null, "Prioridade");
        var sortOpt = new Option<double?>("--sort-order", () => null, "Ordem");
        var dueOpt = new Option<DateTime?>("--due-date", () => null, "Data de vencimento");
        var startOpt = new Option<DateTime?>("--start-date", () => null, "Data de início");
        var versionOpt = new Option<long?>("--version", () => null, "Versão para otimista");

        var list = new Command("list", "Listar issues")
        {
            projectOpt, statusOpt, archivedOpt, assigneeOpt, labelOpt,
        };
        list.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var query = new List<string>();
            AddIfSet(query, ctx, "projectId", projectOpt);
            AddIfSet(query, ctx, "status", statusOpt);
            if (ctx.ParseResult.GetValueForOption(archivedOpt) is { } a)
            {
                query.Add($"archived={a.ToString().ToLowerInvariant()}");
            }
            AddIfSet(query, ctx, "assigneeId", assigneeOpt);
            AddIfSet(query, ctx, "label", labelOpt);

            var path = "/api/tasks" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var result = await client.GetAsync(path, ct);
            return await WriteOutputAsync(ctx, result, "tasks");
        });

        var getArg = new Argument<string>("identifier", "Identificador da issue");
        var get = new Command("get", "Obter issue") { getArg };
        get.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(getArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var result = await client.GetAsync($"/api/tasks/{id}", ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var create = new Command("create", "Criar issue")
        {
            projectOpt, titleOpt, descriptionOpt, statusOpt, priorityOpt, sortOpt, dueOpt, startOpt,
        };
        create.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var config = CliConfigService.Load();
            var projectId = ctx.ParseResult.GetValueForOption(projectOpt) ?? config.CurrentProject;
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new CliException(2, "Informe --project ou use 'taskctl project map'.");
            }

            var title = ctx.ParseResult.GetValueForOption(titleOpt);
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new CliException(2, "Informe --title.");
            }

            var payload = new CreateTaskRequest(
                projectId,
                title,
                ctx.ParseResult.GetValueForOption(descriptionOpt),
                ctx.ParseResult.GetValueForOption(statusOpt),
                ctx.ParseResult.GetValueForOption(priorityOpt),
                null,
                null,
                ctx.ParseResult.GetValueForOption(sortOpt),
                ctx.ParseResult.GetValueForOption(startOpt),
                ctx.ParseResult.GetValueForOption(dueOpt));

            var result = await client.PostAsync("/api/tasks", payload, ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var updateArg = new Argument<string>("identifier", "Identificador da issue");
        var update = new Command("update", "Atualizar issue")
        {
            updateArg, titleOpt, descriptionOpt, statusOpt, priorityOpt, versionOpt,
        };
        update.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(updateArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var current = await client.GetAsync($"/api/tasks/{id}", ct);
            var version = ctx.ParseResult.GetValueForOption(versionOpt) ?? current?["task"]?["version"]?.GetValue<long>() ?? 0L;

            var patch = new TaskPatch(
                Title: ctx.ParseResult.GetValueForOption(titleOpt),
                Description: ctx.ParseResult.GetValueForOption(descriptionOpt),
                Status: ctx.ParseResult.GetValueForOption(statusOpt),
                Priority: ctx.ParseResult.GetValueForOption(priorityOpt));

            var payload = new UpdateTaskRequest(version, patch);
            var result = await client.PatchAsync($"/api/tasks/{id}", payload, ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var moveArg = new Argument<string>("identifier", "Identificador da issue");
        var moveStatusOpt = new Option<string>("--status", "Status de destino") { IsRequired = true };
        var moveSortOpt = new Option<double?>("--sort-order", () => null, "Ordem");
        var move = new Command("move", "Mover issue")
        {
            moveArg, moveStatusOpt, moveSortOpt,
        };
        move.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(moveArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var status = ctx.ParseResult.GetValueForOption(moveStatusOpt)!;
            var payload = new MoveTaskRequest(status, ctx.ParseResult.GetValueForOption(moveSortOpt));
            var result = await client.PostAsync($"/api/tasks/{id}/move", payload, ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var archiveArg = new Argument<string>("identifier", "Identificador da issue");
        var archive = new Command("archive", "Arquivar issue") { archiveArg };
        archive.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(archiveArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var result = await client.PostAsync($"/api/tasks/{id}/archive", null, ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var restoreArg = new Argument<string>("identifier", "Identificador da issue");
        var restore = new Command("restore", "Restaurar issue") { restoreArg };
        restore.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(restoreArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var result = await client.PostAsync($"/api/tasks/{id}/restore", null, ct);
            return await WriteOutputAsync(ctx, result, "task");
        });

        var relSourceArg = new Argument<string>("identifier", "Identificador da issue de origem");
        var relActionArg = new Argument<string>("action", "add ou remove");
        var relTypeArg = new Argument<string>("type", "Tipo da relação");
        var relTargetArg = new Argument<string>("target", "Identificador da issue de destino");
        var relation = new Command("relation", "Gerenciar relações")
        {
            relSourceArg, relActionArg, relTypeArg, relTargetArg,
        };
        relation.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(relSourceArg);
            var action = ctx.ParseResult.GetValueForArgument(relActionArg);
            var type = ctx.ParseResult.GetValueForArgument(relTypeArg);
            var target = ctx.ParseResult.GetValueForArgument(relTargetArg);
            var sourceId = await ResolveTaskIdAsync(client, identifier, ct);
            if (sourceId is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var targetId = await ResolveTaskIdAsync(client, target, ct);
            if (targetId is null)
            {
                throw new CliException(2, $"Issue '{target}' não encontrada.");
            }

            if (action.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new CreateRelationRequest(targetId, type);
                var result = await client.PostAsync($"/api/tasks/{sourceId}/relations", payload, ct);
                return await WriteOutputAsync(ctx, result, "relation");
            }

            if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                await client.DeleteAsync($"/api/tasks/{sourceId}/relations/{type}/{targetId}", ct);
                return await WriteOutputAsync(ctx, null);
            }

            throw new CliException(2, "Ação deve ser 'add' ou 'remove'.");
        });

        issue.AddCommand(list);
        issue.AddCommand(get);
        issue.AddCommand(create);
        issue.AddCommand(update);
        issue.AddCommand(move);
        issue.AddCommand(archive);
        issue.AddCommand(restore);
        issue.AddCommand(relation);
        return issue;
    }

    private static void AddIfSet(List<string> query, InvocationContext ctx, string name, Option<string?> option)
    {
        var value = ctx.ParseResult.GetValueForOption(option);
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }

    private static Command CreateCommentCommand()
    {
        var comment = new Command("comment", "Gerenciar comentários");

        var taskArg = new Argument<string>("identifier", "Identificador da issue");
        var list = new Command("list", "Listar comentários") { taskArg };
        list.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(taskArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var result = await client.GetAsync($"/api/tasks/{id}/comments", ct);
            return await WriteOutputAsync(ctx, result, "comments");
        });

        var addBodyArg = new Argument<string>("body", "Texto do comentário");
        var add = new Command("add", "Adicionar comentário") { taskArg, addBodyArg };
        add.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(taskArg);
            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            var body = ctx.ParseResult.GetValueForArgument(addBodyArg);
            var payload = new CreateCommentRequest(body);
            var result = await client.PostAsync($"/api/tasks/{id}/comments", payload, ct);
            return await WriteOutputAsync(ctx, result, "comment");
        });

        var commentIdArg = new Argument<string>("commentId", "ID do comentário");
        var updateBodyArg = new Argument<string>("body", "Novo texto");
        var update = new Command("update", "Atualizar comentário") { commentIdArg, updateBodyArg };
        update.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var commentId = ctx.ParseResult.GetValueForArgument(commentIdArg);
            var (taskId, resolvedCommentId) = await ResolveCommentAsync(client, commentId, ct);
            if (taskId is null || resolvedCommentId is null)
            {
                throw new CliException(2, $"Comentário '{commentId}' não encontrado.");
            }

            var body = ctx.ParseResult.GetValueForArgument(updateBodyArg);
            var payload = new UpdateCommentRequest(body);
            var result = await client.PatchAsync($"/api/tasks/{taskId}/comments/{resolvedCommentId}", payload, ct);
            return await WriteOutputAsync(ctx, result, "comment");
        });

        var delete = new Command("delete", "Remover comentário") { commentIdArg };
        delete.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var commentId = ctx.ParseResult.GetValueForArgument(commentIdArg);
            var (taskId, resolvedCommentId) = await ResolveCommentAsync(client, commentId, ct);
            if (taskId is null || resolvedCommentId is null)
            {
                throw new CliException(2, $"Comentário '{commentId}' não encontrado.");
            }

            await client.DeleteAsync($"/api/tasks/{taskId}/comments/{resolvedCommentId}", ct);
            return await WriteOutputAsync(ctx, null);
        });

        comment.AddCommand(list);
        comment.AddCommand(add);
        comment.AddCommand(update);
        comment.AddCommand(delete);
        return comment;
    }

    private static Command CreateAttachmentCommand()
    {
        var attachment = new Command("attachment", "Gerenciar anexos");

        var uploadTaskArg = new Argument<string>("identifier", "Identificador da issue");
        var fileArg = new Argument<string>("file", "Caminho do arquivo");
        var kindOpt = new Option<string>("--kind", () => "file", "Tipo do anexo");
        var upload = new Command("upload", "Enviar anexo") { uploadTaskArg, fileArg, kindOpt };
        upload.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var identifier = ctx.ParseResult.GetValueForArgument(uploadTaskArg);
            var path = ctx.ParseResult.GetValueForArgument(fileArg);
            if (!File.Exists(path))
            {
                throw new CliException(2, $"Arquivo '{path}' não encontrado.");
            }

            var id = await ResolveTaskIdAsync(client, identifier, ct);
            if (id is null)
            {
                throw new CliException(2, $"Issue '{identifier}' não encontrada.");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(id), "taskId");
            content.Add(new StringContent(ctx.ParseResult.GetValueForOption(kindOpt)!), "kind");
            using var stream = File.OpenRead(path);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", Path.GetFileName(path));

            var result = await client.PostMultipartAsync("/api/attachments", content, ct);
            return await WriteOutputAsync(ctx, result, "attachment");
        });

        var downloadIdArg = new Argument<string>("attachmentId", "ID do anexo");
        var outputArg = new Argument<string>("output", "Caminho de saída");
        var download = new Command("download", "Baixar anexo") { downloadIdArg, outputArg };
        download.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var id = ctx.ParseResult.GetValueForArgument(downloadIdArg);
            var output = ctx.ParseResult.GetValueForArgument(outputArg);
            using var stream = File.Create(output);
            await client.DownloadAsync($"/api/attachments/{id}/download", stream, ct);
            var node = new JsonObject { ["downloaded"] = output };
            return await WriteOutputAsync(ctx, node);
        });

        attachment.AddCommand(upload);
        attachment.AddCommand(download);
        return attachment;
    }

    private static Command CreateCloudCommand()
    {
        var cloud = new Command("cloud", "Gerenciar conexão com a nuvem");

        var cloudUrlArg = new Argument<string?>("url", () => null, "URL da nuvem");

        var login = new Command("login", "Conectar à nuvem") { cloudUrlArg };
        login.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var config = CliConfigService.Load();
            var url = ctx.ParseResult.GetValueForArgument(cloudUrlArg);
            if (!string.IsNullOrWhiteSpace(url))
            {
                config.CloudUrl = url;
                CliConfigService.Save(config);
            }

            await client.PutAsync("/api/local/cloud-session", new { connected = true }, ct);
            var node = new JsonObject
            {
                ["connected"] = true,
                ["cloudUrl"] = config.CloudUrl,
            };
            return await WriteOutputAsync(ctx, node);
        });

        var status = new Command("status", "Status da nuvem");
        status.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var result = await client.GetAsync("/api/local/cloud-session", ct);
            return await WriteOutputAsync(ctx, result);
        });

        var logout = new Command("logout", "Desconectar da nuvem");
        logout.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var client = CreateClient(ctx);
            var config = CliConfigService.Load();
            await client.PutAsync("/api/local/cloud-session", new { connected = false }, ct);
            config.CloudUrl = null;
            CliConfigService.Save(config);
            var node = new JsonObject { ["connected"] = false };
            return await WriteOutputAsync(ctx, node);
        });

        cloud.AddCommand(login);
        cloud.AddCommand(status);
        cloud.AddCommand(logout);
        return cloud;
    }

    private static Command CreateContextCommand()
    {
        var context = new Command("context", "Contexto atual");

        var current = new Command("current", "Exibir contexto");
        current.Handler = new AsyncHandler(async (ctx, ct) =>
        {
            var config = CliConfigService.Load();
            var url = GetBaseUrl(ctx);
            var node = new JsonObject
            {
                ["baseUrl"] = url,
                ["currentProject"] = config.CurrentProject,
                ["currentWorkspace"] = config.CurrentWorkspace,
                ["cloudUrl"] = config.CloudUrl,
            };
            return await WriteOutputAsync(ctx, node);
        });

        context.AddCommand(current);
        return context;
    }

    private sealed class AsyncHandler : ICommandHandler
    {
        private readonly Func<InvocationContext, CancellationToken, Task<int>> _action;

        public AsyncHandler(Func<InvocationContext, CancellationToken, Task<int>> action)
        {
            _action = action;
        }

        public int Invoke(InvocationContext context)
            => _action(context, context.GetCancellationToken()).GetAwaiter().GetResult();

        public Task<int> InvokeAsync(InvocationContext context)
            => _action(context, context.GetCancellationToken());
    }
}
