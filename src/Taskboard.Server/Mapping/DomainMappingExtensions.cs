using Taskboard.Domain.Entities;
using Taskboard.Dtos;

namespace Taskboard.Server.Mapping;

public static class DomainMappingExtensions
{
    public static ProjectDto ToDto(this Project project, long issueCount)
        => new(
            project.Id.Value,
            project.Name,
            project.WorkspacePath,
            project.Labels.ToList().AsReadOnly(),
            issueCount,
            project.CreatedAt,
            project.UpdatedAt);

    public static TaskDto ToDto(this Taskboard.Domain.Entities.Task task)
        => new(
            task.Id.Value,
            task.Identifier.Value,
            task.ProjectId.Value,
            task.Title,
            task.Description,
            task.Status.Value,
            task.Priority.Value,
            task.Labels.ToList().AsReadOnly(),
            task.SortOrder,
            task.ThreadBinding,
            task.Creator,
            task.Assignee,
            task.WorkflowId,
            task.GitBranch,
            task.WorktreePath,
            task.WorktreeBranch,
            task.StartDate,
            task.DueDate,
            task.Recurrence,
            task.ExternalSource,
            task.ExternalOrigin,
            task.ExternalId,
            task.ExternalKey,
            task.ExternalUrl,
            task.ArchivedAt,
            task.CreatedAt,
            task.UpdatedAt,
            task.Version);

    public static CommentDto ToDto(this Comment comment)
        => new(
            comment.Id.Value,
            comment.TaskId.Value,
            comment.Body,
            comment.Author,
            comment.ThreadId,
            comment.CreatedAt,
            comment.UpdatedAt);

    public static AttachmentDto ToDto(this Attachment attachment)
        => new(
            attachment.Id.Value,
            attachment.TaskId.Value,
            attachment.CommentId?.Value,
            attachment.Kind.Value,
            attachment.Filename,
            attachment.ContentType,
            attachment.Size,
            attachment.Path,
            attachment.CreatedAt);
}
