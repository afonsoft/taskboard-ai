using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskboard;
using Taskboard.Domain.Entities;
using Taskboard.Domain.Events;
using Taskboard.Dtos;
using Taskboard.EntityFrameworkCore;
using Taskboard.EntityFrameworkCore.Data;
using Taskboard.Json;
using Taskboard.Repositories;
using Taskboard.Requests;
using Taskboard.Server.Mapping;
using Taskboard.Server.Middleware;
using Taskboard.Server.Serialization;
using Taskboard.Server.Services;
using Taskboard.ValueObjects;
using DomainTask = Taskboard.Domain.Entities.Task;
using TaskStatus = Taskboard.ValueObjects.TaskStatus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSingleton<IEventStreamService, InMemoryEventStreamService>();

var dataDir = Environment.GetEnvironmentVariable("CODEX_TASKBOARD_DATA_DIR")
              ?? Path.Combine(builder.Environment.ContentRootPath, ".data");
Directory.CreateDirectory(dataDir);
var connectionString = $"Data Source={Path.Combine(dataDir, "taskboard.sqlite")}";

builder.Services.AddTaskboardEntityFrameworkCore(connectionString);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new StringIdJsonConverterFactory());
    options.SerializerOptions.Converters.Add(new StringValueObjectJsonConverterFactory());
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Dev");

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskboardDbContext>();
    await dbContext.Database.MigrateAsync();
}

var api = app.MapGroup("/api");

app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

api.MapGet("/meta", () => Results.Ok(new { name = "taskboard", version = "1.0.0" }));

api.MapGet("/client-storage", () => Results.Ok(new { data = (string?)null }));
api.MapPut("/client-storage", (object? _) => Results.NoContent());

api.MapGet("/local/codex-thread-progress", () => Results.Ok(new { progress = (string?)null }));
api.MapGet("/local/host-runtime", () => Results.Ok(new { runtime = "dotnet", version = Environment.Version.ToString() }));
api.MapGet("/local/cloud-session", () => Results.Ok(new { connected = false }));
api.MapPut("/local/cloud-session", (object? _) => Results.Ok(new { connected = false }));
api.MapGet("/local/jira-connection", () => Results.Ok(new { connected = false }));
api.MapPost("/local/jira-connection", (object? _) => Results.Ok(new { connected = false }));
api.MapPost("/local/jira-connection/sync", () => Results.Accepted());
api.MapGet("/local/ai/catalog", () => Results.Ok(new { models = Array.Empty<object>() }));
api.MapPost("/local/ai/catalog", (object? _) => Results.Ok(new { models = Array.Empty<object>() }));
api.MapGet("/local/ai/composer/candidates", () => Results.Ok(new { candidates = Array.Empty<object>() }));
api.MapPost("/local/ai/composer/rebind", (object? _) => Results.NoContent());
api.MapGet("/local/ai/threads", () => Results.Ok(new { threads = Array.Empty<object>() }));
api.MapPost("/local/ai/threads", (object? _) => Results.Ok(new { threadId = (string?)null }));
api.MapGet("/device-workspaces", () => Results.Ok(new { workspaces = Array.Empty<object>() }));
api.MapPut("/device-workspaces", (object? _) => Results.NoContent());
api.MapGet("/workflow-capabilities", () => Results.Ok(new { capabilities = Array.Empty<object>() }));
api.MapPut("/workflow-capabilities", (object? _) => Results.NoContent());

var projects = api.MapGroup("/projects");

projects.MapGet("", async (IRepository<Project> projectRepo, IRepository<DomainTask> taskRepo, CancellationToken ct) =>
{
    var projectsList = await projectRepo.ListAsync(ct);
    var allTasks = await taskRepo.ListAsync(ct);

    var dtos = projectsList
        .Select(p => p.ToDto(allTasks.Count(t => t.ProjectId == p.Id && !t.ArchivedAt.HasValue)))
        .ToList();

    return Results.Ok(new { projects = dtos });
});

projects.MapPost("", async (CreateProjectRequest request, IRepository<Project> projectRepo, IEventStreamService events, CancellationToken ct) =>
{
    var id = string.IsNullOrWhiteSpace(request.Id) ? ProjectId.NewGuid() : ProjectId.From(request.Id);

    var existing = await projectRepo.Query.AnyAsync(p => p.Id == id, ct);
    if (existing)
    {
        return Results.Conflict(new { error = new { code = "PROJECT_EXISTS", message = $"Project '{id.Value}' already exists." } });
    }

    var project = Project.Create(id, request.Name, request.WorkspacePath);
    await projectRepo.AddAsync(project, ct);
    await projectRepo.SaveChangesAsync(ct);

    return Results.Created($"/api/projects/{project.Id.Value}", new { project = project.ToDto(0L) });
});

var tasks = api.MapGroup("/tasks");

tasks.MapGet("", async (
    HttpRequest request,
    IRepository<DomainTask> taskRepo,
    IRepository<Project> projectRepo,
    CancellationToken ct) =>
{
    var projectId = request.Query["projectId"].FirstOrDefault();
    var status = request.Query["status"].FirstOrDefault();
    var archivedValue = request.Query["archived"].FirstOrDefault();
    var q = request.Query["q"].FirstOrDefault();
    var assigneeId = request.Query["assigneeId"].FirstOrDefault();
    var label = request.Query["label"].FirstOrDefault();

    var archived = bool.TryParse(archivedValue, out var archivedParsed) ? archivedParsed : (bool?)null;

    var allTasks = await taskRepo.ListAsync(ct);
    IEnumerable<DomainTask> filtered = allTasks;

    if (!string.IsNullOrWhiteSpace(projectId))
    {
        filtered = filtered.Where(t => t.ProjectId.Value == projectId);
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        filtered = filtered.Where(t => t.Status.Value == status);
    }

    if (archived == false)
    {
        filtered = filtered.Where(t => !t.ArchivedAt.HasValue);
    }
    else if (archived == true)
    {
        filtered = filtered.Where(t => t.ArchivedAt.HasValue);
    }

    if (!string.IsNullOrWhiteSpace(q))
    {
        var lower = q.ToLowerInvariant();
        filtered = filtered.Where(t =>
            t.Title.ToLowerInvariant().Contains(lower)
            || (t.Description?.ToLowerInvariant().Contains(lower) ?? false)
            || t.Identifier.Value.ToLowerInvariant().Contains(lower));
    }

    if (!string.IsNullOrWhiteSpace(assigneeId))
    {
        filtered = filtered.Where(t => t.Assignee?.Id == assigneeId);
    }

    if (!string.IsNullOrWhiteSpace(label))
    {
        filtered = filtered.Where(t => t.Labels.Contains(label, StringComparer.Ordinal));
    }

    var result = filtered
        .OrderBy(t => t.SortOrder ?? double.MaxValue)
        .ThenBy(t => t.CreatedAt)
        .Select(t => t.ToDto())
        .ToList();

    ProjectDto? projectDto = null;
    if (!string.IsNullOrWhiteSpace(projectId))
    {
        var project = await projectRepo.GetAsync(new ProjectId(projectId), ct);
        projectDto = project?.ToDto(result.Count);
    }

    return Results.Ok(new TaskListDto(result, projectDto));
});

tasks.MapPost("", async (
    CreateTaskRequest request,
    IRepository<Project> projectRepo,
    IRepository<DomainTask> taskRepo,
    IEventStreamService events,
    CancellationToken ct) =>
{
    var project = await projectRepo.GetAsync(new ProjectId(request.ProjectId), ct);
    if (project is null)
    {
        return Results.NotFound(new { error = new { code = "PROJECT_NOT_FOUND", message = $"Project '{request.ProjectId}' not found." } });
    }

    var status = TaskStatus.From(request.Status ?? "todo");
    var priority = TaskPriority.From(request.Priority ?? "medium");
    var identifier = project.GenerateTaskIdentifier();
    var task = DomainTask.Create(
        TaskId.NewGuid(),
        identifier,
        project.Id,
        request.Title,
        request.Description,
        status,
        priority,
        request.Creator ?? Actor.LocalUser(),
        request.Labels,
        null,
        DateTime.UtcNow);

    if (request.SortOrder.HasValue || request.StartDate.HasValue || request.DueDate.HasValue)
    {
        task.ApplyPatch(new TaskPatch(SortOrder: request.SortOrder, StartDate: request.StartDate, DueDate: request.DueDate), task.Version);
    }

    await taskRepo.AddAsync(task, ct);
    await projectRepo.UpdateAsync(project, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task, project);

    return Results.Created($"/api/tasks/{task.Id.Value}", new { task = task.ToDto() });
});

tasks.MapGet("{id}", async (string id, IRepository<DomainTask> taskRepo, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    return Results.Ok(new { task = task.ToDto() });
});

tasks.MapPatch("{id}", async (string id, UpdateTaskRequest request, IRepository<DomainTask> taskRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    task.ApplyPatch(request.Changes, request.Version);
    await taskRepo.UpdateAsync(task, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task);

    return Results.Ok(new { task = task.ToDto() });
});

tasks.MapDelete("{id}", async (string id, long version, IRepository<DomainTask> taskRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    if (task.Version != version)
    {
        return Results.Conflict(new { error = new { code = TaskboardDomainErrorCodes.VersionConflict, message = $"Expected version {version} but found {task.Version}." } });
    }

    task.Delete(Actor.LocalUser());
    await taskRepo.DeleteAsync(task, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task);

    return Results.NoContent();
});

tasks.MapPost("{id}/move", async (string id, [FromBody] MoveTaskRequest request, IRepository<DomainTask> taskRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    task.Move(TaskStatus.From(request.Status), request.SortOrder, Actor.LocalUser());
    await taskRepo.UpdateAsync(task, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task);

    return Results.Ok(new { task = task.ToDto() });
});

tasks.MapPost("{id}/archive", async (string id, IRepository<DomainTask> taskRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    task.Archive(Actor.LocalUser());
    await taskRepo.UpdateAsync(task, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task);

    return Results.Ok(new { task = task.ToDto() });
});

tasks.MapPost("{id}/restore", async (string id, IRepository<DomainTask> taskRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    task.Restore(Actor.LocalUser());
    await taskRepo.UpdateAsync(task, ct);
    await taskRepo.SaveChangesAsync(ct);

    await PublishEventsAsync(events, task);

    return Results.Ok(new { task = task.ToDto() });
});

tasks.MapGet("{id}/comments", async (string id, IRepository<Comment> commentRepo, CancellationToken ct) =>
{
    var comments = await commentRepo.Query
        .Where(c => c.TaskId == new TaskId(id))
        .OrderBy(c => c.CreatedAt)
        .ToListAsync(ct);

    return Results.Ok(new { comments = comments.Select(c => c.ToDto()) });
});

tasks.MapPost("{id}/comments", async (string id, CreateCommentRequest request, IRepository<DomainTask> taskRepo, IRepository<Comment> commentRepo, IEventStreamService events, CancellationToken ct) =>
{
    var task = await taskRepo.GetAsync(new TaskId(id), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    var comment = Comment.Create(CommentId.NewGuid(), task.Id, request.Body, Actor.LocalUser());
    await commentRepo.AddAsync(comment, ct);
    await commentRepo.SaveChangesAsync(ct);

    await events.PublishAsync(new ServerSentEvent("comment.added", new CommentAddedDomainEvent(task.Id, comment.Id)), ct);

    return Results.Created($"/api/tasks/{id}/comments/{comment.Id.Value}", new { comment = comment.ToDto() });
});

tasks.MapPatch("{taskId}/comments/{commentId}", async (string taskId, string commentId, UpdateCommentRequest request, IRepository<Comment> commentRepo, CancellationToken ct) =>
{
    var comment = await commentRepo.GetAsync(new CommentId(commentId), ct);
    if (comment is null || comment.TaskId.Value != taskId)
    {
        return Results.NotFound(new { error = new { code = "COMMENT_NOT_FOUND", message = $"Comment '{commentId}' not found." } });
    }

    comment.Edit(request.Body);
    await commentRepo.UpdateAsync(comment, ct);
    await commentRepo.SaveChangesAsync(ct);

    return Results.Ok(new { comment = comment.ToDto() });
});

tasks.MapDelete("{taskId}/comments/{commentId}", async (string taskId, string commentId, IRepository<Comment> commentRepo, CancellationToken ct) =>
{
    var comment = await commentRepo.GetAsync(new CommentId(commentId), ct);
    if (comment is null || comment.TaskId.Value != taskId)
    {
        return Results.NotFound(new { error = new { code = "COMMENT_NOT_FOUND", message = $"Comment '{commentId}' not found." } });
    }

    await commentRepo.DeleteAsync(comment, ct);
    await commentRepo.SaveChangesAsync(ct);

    return Results.NoContent();
});

tasks.MapGet("{id}/activities", async (string id, IRepository<TaskActivity> activityRepo, CancellationToken ct) =>
{
    var activities = await activityRepo.Query
        .Where(a => a.TaskId == new TaskId(id))
        .OrderBy(a => a.Timestamp)
        .ToListAsync(ct);

    return Results.Ok(new { activities = activities.Select(a => new { a.Id, a.TaskId, a.Actor, a.Changes, a.Timestamp }) });
});

tasks.MapGet("{id}/relations", async (string id, IRepository<TaskRelation> relationRepo, CancellationToken ct) =>
{
    var relations = await relationRepo.Query
        .Where(r => r.SourceTaskId == new TaskId(id) || r.TargetTaskId == new TaskId(id))
        .ToListAsync(ct);

    return Results.Ok(new { relations = relations.Select(r => new { r.Id, SourceTaskId = r.SourceTaskId.Value, TargetTaskId = r.TargetTaskId.Value, Type = r.RelationType.Value, r.CreatedAt }) });
});

tasks.MapPost("{id}/relations", async (string id, [FromBody] CreateRelationRequest request, IRepository<DomainTask> taskRepo, IRepository<TaskRelation> relationRepo, CancellationToken ct) =>
{
    var sourceTask = await taskRepo.GetAsync(new TaskId(id), ct);
    if (sourceTask is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{id}' not found." } });
    }

    var relation = TaskRelation.Create(
        new TaskId(id),
        new TaskId(request.TargetTaskId),
        RelationType.From(request.Type));

    await relationRepo.AddAsync(relation, ct);
    await relationRepo.SaveChangesAsync(ct);

    return Results.Created($"/api/tasks/{id}/relations/{request.Type}/{request.TargetTaskId}", new { relation = new { relation.Id, SourceTaskId = relation.SourceTaskId.Value, TargetTaskId = relation.TargetTaskId.Value, Type = relation.RelationType.Value, relation.CreatedAt } });
});

tasks.MapDelete("{id}/relations/{type}/{targetTaskId}", async (string id, string type, string targetTaskId, IRepository<TaskRelation> relationRepo, CancellationToken ct) =>
{
    var relation = await relationRepo.Query.FirstOrDefaultAsync(
        r => r.SourceTaskId == new TaskId(id)
             && r.TargetTaskId == new TaskId(targetTaskId)
             && r.RelationType == RelationType.From(type),
        ct);

    if (relation is null)
    {
        return Results.NotFound(new { error = new { code = "RELATION_NOT_FOUND", message = "Relation not found." } });
    }

    await relationRepo.DeleteAsync(relation, ct);
    await relationRepo.SaveChangesAsync(ct);

    return Results.NoContent();
});

var attachments = api.MapGroup("/attachments");

attachments.MapPost("", async (
    HttpRequest request,
    IRepository<DomainTask> taskRepo,
    IRepository<Attachment> attachmentRepo,
    IEventStreamService events,
    CancellationToken ct) =>
{
    var form = await request.ReadFormAsync(ct);
    var file = form.Files.FirstOrDefault();
    var taskId = form["taskId"].FirstOrDefault();
    var commentIdValue = form["commentId"].FirstOrDefault();
    var kindValue = form["kind"].FirstOrDefault() ?? "file";

    if (file is null || string.IsNullOrWhiteSpace(taskId))
    {
        return Results.BadRequest(new { error = new { code = "INVALID_ATTACHMENT", message = "Missing file or taskId." } });
    }

    var task = await taskRepo.GetAsync(new TaskId(taskId), ct);
    if (task is null)
    {
        return Results.NotFound(new { error = new { code = "TASK_NOT_FOUND", message = $"Task '{taskId}' not found." } });
    }

    var attachmentId = AttachmentId.NewGuid();
    var attachmentDir = Path.Combine(dataDir, "attachments", attachmentId.Value);
    Directory.CreateDirectory(attachmentDir);
    var filePath = Path.Combine(attachmentDir, file.FileName);

    await using (var stream = File.Create(filePath))
    {
        await file.CopyToAsync(stream, ct);
    }

    CommentId? commentId = null;
    if (!string.IsNullOrWhiteSpace(commentIdValue))
    {
        commentId = new CommentId(commentIdValue);
    }

    var attachment = Attachment.Create(
        attachmentId,
        task.Id,
        new AttachmentKind(kindValue),
        file.FileName,
        file.ContentType,
        file.Length,
        filePath,
        DateTime.UtcNow,
        commentId);

    await attachmentRepo.AddAsync(attachment, ct);
    await attachmentRepo.SaveChangesAsync(ct);

    await events.PublishAsync(new ServerSentEvent("attachment.created", attachment.ToDto()), ct);

    return Results.Created($"/api/attachments/{attachment.Id.Value}", new { attachment = attachment.ToDto() });
});

attachments.MapGet("{id}/content", async (string id, IRepository<Attachment> attachmentRepo, CancellationToken ct) =>
{
    var attachment = await attachmentRepo.GetAsync(new AttachmentId(id), ct);
    if (attachment is null || !File.Exists(attachment.Path))
    {
        return Results.NotFound(new { error = new { code = "ATTACHMENT_NOT_FOUND", message = $"Attachment '{id}' not found." } });
    }

    return Results.File(attachment.Path, attachment.ContentType, enableRangeProcessing: true);
});

attachments.MapGet("{id}/download", async (string id, IRepository<Attachment> attachmentRepo, CancellationToken ct) =>
{
    var attachment = await attachmentRepo.GetAsync(new AttachmentId(id), ct);
    if (attachment is null || !File.Exists(attachment.Path))
    {
        return Results.NotFound(new { error = new { code = "ATTACHMENT_NOT_FOUND", message = $"Attachment '{id}' not found." } });
    }

    return Results.File(attachment.Path, attachment.ContentType, attachment.Filename, enableRangeProcessing: true);
});

attachments.MapDelete("{id}", async (string id, IRepository<Attachment> attachmentRepo, IEventStreamService events, CancellationToken ct) =>
{
    var attachment = await attachmentRepo.GetAsync(new AttachmentId(id), ct);
    if (attachment is null)
    {
        return Results.NotFound(new { error = new { code = "ATTACHMENT_NOT_FOUND", message = $"Attachment '{id}' not found." } });
    }

    await attachmentRepo.DeleteAsync(attachment, ct);
    await attachmentRepo.SaveChangesAsync(ct);

    if (File.Exists(attachment.Path))
    {
        File.Delete(attachment.Path);
    }

    await events.PublishAsync(new ServerSentEvent("attachment.deleted", new { attachment.Id.Value }), ct);

    return Results.NoContent();
});

app.MapGet("/api/events", async (HttpResponse response, IEventStreamService eventStream, CancellationToken ct) =>
{
    response.Headers.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";

    await foreach (var ev in eventStream.SubscribeAsync(ct))
    {
        await response.WriteAsync($"event: {ev.Type}\n", ct);
        await response.WriteAsync($"data: {JsonSerializer.Serialize(ev.Payload, ApiJsonOptions.Default)}\n\n", ct);
        await response.Body.FlushAsync(ct);
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

static ServerSentEvent MapDomainEvent(IDomainEvent domainEvent)
{
    return domainEvent switch
    {
        TaskCreatedDomainEvent e => new ServerSentEvent("task.created", e),
        TaskMovedDomainEvent e => new ServerSentEvent("task.moved", e),
        TaskUpdatedDomainEvent e => new ServerSentEvent("task.updated", e),
        TaskArchivedDomainEvent e => new ServerSentEvent("task.archived", e),
        TaskRestoredDomainEvent e => new ServerSentEvent("task.restored", e),
        TaskDeletedDomainEvent e => new ServerSentEvent("task.deleted", e),
        CommentAddedDomainEvent e => new ServerSentEvent("comment.added", e),
        ProjectLabelsUpdatedDomainEvent e => new ServerSentEvent("project.labels_updated", e),
        _ => new ServerSentEvent("domain.event", domainEvent)
    };
}

static async System.Threading.Tasks.Task PublishEventsAsync(IEventStreamService eventStream, params object[] aggregates)
{
    var events = new List<IDomainEvent>();

    foreach (var aggregate in aggregates)
    {
        if (aggregate is Project project)
        {
            events.AddRange(project.DomainEvents);
            project.ClearDomainEvents();
        }
        else if (aggregate is DomainTask task)
        {
            events.AddRange(task.DomainEvents);
            task.ClearDomainEvents();
        }
    }

    foreach (var domainEvent in events)
    {
        await eventStream.PublishAsync(MapDomainEvent(domainEvent));
    }
}


