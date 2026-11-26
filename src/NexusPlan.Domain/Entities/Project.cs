namespace NexusPlan.Domain.Entities;

/// <summary>Proyecto que agrupa un conjunto de tareas (relación 1:N).</summary>
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
