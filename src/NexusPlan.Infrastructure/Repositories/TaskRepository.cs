using Microsoft.EntityFrameworkCore;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;
using NexusPlan.Domain.Repositories;
using NexusPlan.Infrastructure.Data;

namespace NexusPlan.Infrastructure.Repositories;

public class TaskRepository(NexusPlanDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskState? state, TaskPriority? priority, int? projectId, CancellationToken ct = default)
    {
        var query = Filter(db.Tasks.AsNoTracking(), state, priority);
        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        return await query.OrderBy(t => t.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectAsync(int projectId, TaskState? state, TaskPriority? priority, CancellationToken ct = default)
    {
        var query = Filter(db.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId), state, priority);
        return await query.OrderBy(t => t.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetOverdueAsync(DateTime nowUtc, int? projectId, CancellationToken ct = default)
    {
        var query = db.Tasks.AsNoTracking().Where(t => t.DueDate < nowUtc && t.State != TaskState.Completed);
        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        return await query.OrderBy(t => t.DueDate).ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        db.Tasks.Update(task);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken ct = default)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
    }

    private static IQueryable<TaskItem> Filter(IQueryable<TaskItem> query, TaskState? state, TaskPriority? priority)
    {
        if (state.HasValue) query = query.Where(t => t.State == state.Value);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
        return query;
    }
}
