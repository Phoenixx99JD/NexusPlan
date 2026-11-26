using Microsoft.EntityFrameworkCore;
using NexusPlan.Domain.Entities;

namespace NexusPlan.Infrastructure.Data;

public class NexusPlanDbContext(DbContextOptions<NexusPlanDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexusPlanDbContext).Assembly);
    }
}
