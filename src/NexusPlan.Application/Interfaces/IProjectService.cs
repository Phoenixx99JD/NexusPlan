using NexusPlan.Application.Dtos;

namespace NexusPlan.Application.Interfaces;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ProjectResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProjectResponse> CreateAsync(ProjectRequest request, CancellationToken ct = default);
    Task<ProjectResponse> UpdateAsync(int id, ProjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ProjectProgressResponse> GetProgressAsync(int id, CancellationToken ct = default);
}
