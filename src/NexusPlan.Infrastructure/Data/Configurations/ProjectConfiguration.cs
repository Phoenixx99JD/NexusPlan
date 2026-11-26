using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusPlan.Domain.Entities;

namespace NexusPlan.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> e)
    {
        e.ToTable("Projects");
        e.HasKey(p => p.Id);
        e.Property(p => p.Name).IsRequired().HasMaxLength(150);
        e.Property(p => p.Description).HasMaxLength(1000);
        e.Property(p => p.StartDate).IsRequired();
        e.Property(p => p.EndDate).IsRequired();
        e.HasIndex(p => p.Name);

        e.HasMany(p => p.Tasks)
         .WithOne(t => t.Project)
         .HasForeignKey(t => t.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
