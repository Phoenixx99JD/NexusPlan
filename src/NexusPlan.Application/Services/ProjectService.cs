using NexusPlan.Application.Dtos;
using NexusPlan.Application.Interfaces;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;
using NexusPlan.Domain.Exceptions;
using NexusPlan.Domain.Repositories;

namespace NexusPlan.Application.Services;

public class ProjectService(IProjectRepository projects) : IProjectService
{
    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await projects.GetAllWithTasksAsync(ct);
        return list.Select(ToResponse).ToList();
    }

    public async Task<ProjectResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await projects.GetByIdWithTasksAsync(id, ct)
            ?? throw new NotFoundException($"No existe el proyecto con ID {id}.");

        return ToResponse(project);
    }

    public async Task<ProjectResponse> CreateAsync(ProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.StartDate!.Value,
            EndDate = request.EndDate!.Value
        };
        await projects.AddAsync(project, ct);
        return new ProjectResponse(project.Id, project.Name, project.Description, project.StartDate, project.EndDate, 0, 0, 0m);
    }

    public async Task<ProjectResponse> UpdateAsync(int id, ProjectRequest request, CancellationToken ct = default)
    {
        var project = await projects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe el proyecto con ID {id}.");

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.StartDate = request.StartDate!.Value;
        project.EndDate = request.EndDate!.Value;
        await projects.UpdateAsync(project, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await projects.GetByIdWithTasksAsync(id, ct)
            ?? throw new NotFoundException($"No existe el proyecto con ID {id}.");

        var unfinished = project.Tasks.Count(t => t.State != TaskState.Completed);
        if (unfinished > 0)
        {
            throw new BusinessRuleException(
                $"No se puede eliminar el proyecto {id} porque tiene {unfinished} tarea(s) pendiente(s) o en progreso.");
        }

        await projects.DeleteAsync(project, ct);
    }

    public async Task<ProjectProgressResponse> GetProgressAsync(int id, CancellationToken ct = default)
    {
        var project = await projects.GetByIdWithTasksAsync(id, ct)
            ?? throw new NotFoundException($"No existe el proyecto con ID {id}.");

        var total = project.Tasks.Count;
        var completed = project.Tasks.Count(t => t.State == TaskState.Completed);
        var inProgress = project.Tasks.Count(t => t.State == TaskState.InProgress);
        var pending = project.Tasks.Count(t => t.State == TaskState.Pending);

        return new ProjectProgressResponse(
            project.Id,
            project.Name,
            total,
            completed,
            inProgress,
            pending,
            ProgressCalculator.Calculate(completed, total));
    }

    private static ProjectResponse ToResponse(Project p)
    {
        var total = p.Tasks.Count;
        var completed = p.Tasks.Count(t => t.State == TaskState.Completed);
        return new ProjectResponse(
            p.Id,
            p.Name,
            p.Description,
            p.StartDate,
            p.EndDate,
            total,
            completed,
            ProgressCalculator.Calculate(completed, total));
    }
}

public static class ProgressCalculator
{
    /// <summary>Porcentaje (0-100, 2 decimales) de tareas completadas. 0 si no hay tareas.</summary>
    public static decimal Calculate(int completed, int total) =>
        total == 0 ? 0m : Math.Round(completed * 100m / total, 2, MidpointRounding.AwayFromZero);
}
