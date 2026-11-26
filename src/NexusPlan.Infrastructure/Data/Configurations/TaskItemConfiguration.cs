using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusPlan.Domain.Entities;

namespace NexusPlan.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> e)
    {
        e.ToTable("Tasks");
        e.HasKey(t => t.Id);
        e.Property(t => t.Title).IsRequired().HasMaxLength(200);
        e.Property(t => t.Description).HasMaxLength(2000);
        e.Property(t => t.State).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(t => t.DueDate).IsRequired();
        e.HasIndex(t => new { t.ProjectId, t.State });
        e.HasIndex(t => t.DueDate);
    }
}
