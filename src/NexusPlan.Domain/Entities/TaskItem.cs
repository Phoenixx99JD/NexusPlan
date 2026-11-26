using NexusPlan.Domain.Enums;

namespace NexusPlan.Domain.Entities;

/// <summary>Tarea que pertenece a un único proyecto.</summary>
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskState State { get; set; } = TaskState.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }
}
