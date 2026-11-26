using NexusPlan.Application.Dtos;
using NexusPlan.Application.Interfaces;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;
using NexusPlan.Domain.Exceptions;
using NexusPlan.Domain.Repositories;

namespace NexusPlan.Application.Services;

public class TaskService(ITaskRepository tasks, IProjectRepository projects, TimeProvider clock) : ITaskService
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskState? state, TaskPriority? priority, int? projectId, CancellationToken ct = default)
    {
        var items = await tasks.GetAllAsync(state, priority, projectId, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetByProjectAsync(int projectId, TaskState? state, TaskPriority? priority, CancellationToken ct = default)
    {
        await EnsureProjectExists(projectId, ct);
        var items = await tasks.GetByProjectAsync(projectId, state, priority, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetOverdueAsync(int? projectId, CancellationToken ct = default)
    {
        var items = await tasks.GetOverdueAsync(Now, projectId, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe la tarea con ID {id}.");
        return ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(TaskRequest request, CancellationToken ct = default)
    {
        await EnsureProjectExists(request.ProjectId!.Value, ct);

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            State = request.State,
            Priority = request.Priority!.Value,
            DueDate = request.DueDate!.Value,
            ProjectId = request.ProjectId!.Value
        };
        await tasks.AddAsync(task, ct);
        return ToResponse(task);
    }

    public async Task<TaskResponse> UpdateAsync(int id, TaskRequest request, CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe la tarea con ID {id}.");

        if (task.ProjectId != request.ProjectId)
            await EnsureProjectExists(request.ProjectId!.Value, ct);

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.State = request.State;
        task.Priority = request.Priority!.Value;
        task.DueDate = request.DueDate!.Value;
        task.ProjectId = request.ProjectId!.Value;
        await tasks.UpdateAsync(task, ct);
        return ToResponse(task);
    }

    public async Task<TaskResponse> ChangeStateAsync(int id, TaskState state, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(state))
            throw new ValidationException($"Estado '{state}' inválido.");

        var task = await tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe la tarea con ID {id}.");

        task.State = state;
        await tasks.UpdateAsync(task, ct);
        return ToResponse(task);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"No existe la tarea con ID {id}.");
        await tasks.DeleteAsync(task, ct);
    }

    private async Task EnsureProjectExists(int projectId, CancellationToken ct)
    {
        if (!await projects.ExistsAsync(projectId, ct))
            throw new NotFoundException($"No existe el proyecto con ID {projectId}.");
    }

    private TaskResponse ToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.State, t.Priority, t.DueDate, t.ProjectId,
        IsOverdue: t.State != TaskState.Completed && t.DueDate < Now);
}
