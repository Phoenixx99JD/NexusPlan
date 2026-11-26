using Microsoft.EntityFrameworkCore;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Repositories;
using NexusPlan.Infrastructure.Data;

namespace NexusPlan.Infrastructure.Repositories;

public class ProjectRepository(NexusPlanDbContext db) : IProjectRepository
{
    public async Task<IReadOnlyList<Project>> GetAllWithTasksAsync(CancellationToken ct = default)
        => await db.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

    public async Task<Project?> GetByIdWithTasksAsync(int id, CancellationToken ct = default)
        => await db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
        => await db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await db.Projects.AnyAsync(p => p.Id == id, ct);

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Project project, CancellationToken ct = default)
    {
        db.Projects.Remove(project);
        await db.SaveChangesAsync(ct);
    }
}
