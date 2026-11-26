using NexusPlan.Application.Dtos;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Application.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskState? state, TaskPriority? priority, int? projectId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskResponse>> GetByProjectAsync(int projectId, TaskState? state, TaskPriority? priority, CancellationToken ct = default);
    Task<IReadOnlyList<TaskResponse>> GetOverdueAsync(int? projectId, CancellationToken ct = default);
    Task<TaskResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskResponse> CreateAsync(TaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> UpdateAsync(int id, TaskRequest request, CancellationToken ct = default);
    Task<TaskResponse> ChangeStateAsync(int id, TaskState state, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
