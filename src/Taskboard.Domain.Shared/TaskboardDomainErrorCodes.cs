namespace Taskboard;

public static class TaskboardDomainErrorCodes
{
    public const string InvalidValue = "Taskboard:00001";
    public const string InvalidTaskStatus = "Taskboard:00002";
    public const string InvalidTaskPriority = "Taskboard:00003";
    public const string InvalidAttachmentKind = "Taskboard:00004";
    public const string InvalidRelationType = "Taskboard:00005";
    public const string InvalidSandbox = "Taskboard:00006";
    public const string InvalidTaskIdentifier = "Taskboard:00007";
    public const string InvalidActorType = "Taskboard:00008";
    public const string InvalidRecurrenceUnit = "Taskboard:00009";
    public const string EmptyCommentBody = "Taskboard:00010";
    public const string NegativeAttachmentSize = "Taskboard:00011";
    public const string SelfRelation = "Taskboard:00012";
    public const string DuplicateParent = "Taskboard:00013";
    public const string DuplicateRelated = "Taskboard:00014";
    public const string VersionConflict = "Taskboard:00015";
    public const string TaskArchived = "Taskboard:00016";
    public const string TaskIsJira = "Taskboard:00017";
    public const string ProjectHasActiveTasks = "Taskboard:00018";
    public const string EmptyProjectName = "Taskboard:00019";
    public const string EmptyTaskTitle = "Taskboard:00020";
    public const string TaskTitleTooLong = "Taskboard:00021";
    public const string TaskAlreadyActive = "Taskboard:00022";
    public const string TaskNotArchived = "Taskboard:00023";
    public const string IdentifierTooLong = "Taskboard:00024";
}
