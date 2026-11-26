using NexusPlan.Domain.Entities;

namespace NexusPlan.Domain.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllWithTasksAsync(CancellationToken ct = default);
    Task<Project?> GetByIdWithTasksAsync(int id, CancellationToken ct = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(Project project, CancellationToken ct = default);
}
