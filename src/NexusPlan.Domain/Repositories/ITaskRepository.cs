using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Domain.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskState? state, TaskPriority? priority, int? projectId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByProjectAsync(int projectId, TaskState? state, TaskPriority? priority, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetOverdueAsync(DateTime nowUtc, int? projectId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(TaskItem task, CancellationToken ct = default);
}
