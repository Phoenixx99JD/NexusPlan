using NexusPlan.Application.Dtos;
using NexusPlan.Application.Services;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;
using NexusPlan.Domain.Exceptions;

namespace NexusPlan.Tests.Unit;

public class ProjectServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests() => _sut = new ProjectService(_db.Projects);

    public void Dispose() => _db.Dispose();

    private async Task<int> SeedProjectAsync(params TaskState[] states)
    {
        var project = new Project { Name = "Proyecto", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) };
        foreach (var (state, i) in states.Select((s, i) => (s, i)))
            project.Tasks.Add(new TaskItem { Title = $"Tarea {i}", State = state, Priority = TaskPriority.Medium, DueDate = new DateTime(2026, 7, 1) });
        _db.Context.Projects.Add(project);
        await _db.Context.SaveChangesAsync();
        _db.Context.ChangeTracker.Clear();
        return project.Id;
    }

    [Fact]
    public async Task Create_PersistsProject()
    {
        var result = await _sut.CreateAsync(new ProjectRequest
        {
            Name = "  Nuevo  ", Description = "Desc", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 2, 1)
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Nuevo", result.Name);
        Assert.Equal(0m, result.ProgressPercentage);
    }

    [Fact]
    public async Task GetById_NotFound_Throws()
        => await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(999));

    [Fact]
    public async Task Update_ChangesValues()
    {
        var id = await SeedProjectAsync();
        var result = await _sut.UpdateAsync(id, new ProjectRequest
        {
            Name = "Editado", StartDate = new DateTime(2026, 3, 1), EndDate = new DateTime(2026, 4, 1)
        });
        Assert.Equal("Editado", result.Name);
        Assert.Equal(new DateTime(2026, 3, 1), result.StartDate);
    }

    [Theory]
    [InlineData(new TaskState[0], 0.0)]
    [InlineData(new[] { TaskState.Completed }, 100.0)]
    [InlineData(new[] { TaskState.Completed, TaskState.Pending }, 50.0)]
    [InlineData(new[] { TaskState.Completed, TaskState.Pending, TaskState.InProgress }, 33.33)]
    [InlineData(new[] { TaskState.Completed, TaskState.Completed, TaskState.InProgress }, 66.67)]
    public async Task Progress_IsCalculatedFromCompletedTasks(TaskState[] states, double expected)
    {
        var id = await SeedProjectAsync(states);
        var progress = await _sut.GetProgressAsync(id);
        Assert.Equal((decimal)expected, progress.ProgressPercentage);
        Assert.Equal(states.Length, progress.TotalTasks);
    }

    [Theory]
    [InlineData(TaskState.Pending)]
    [InlineData(TaskState.InProgress)]
    public async Task Delete_WithUnfinishedTasks_ThrowsBusinessRule(TaskState state)
    {
        var id = await SeedProjectAsync(TaskState.Completed, state);
        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.DeleteAsync(id));
        Assert.True(_db.Context.Projects.Any(p => p.Id == id));
    }

    [Fact]
    public async Task Delete_WithOnlyCompletedTasks_RemovesProjectAndTasks()
    {
        var id = await SeedProjectAsync(TaskState.Completed, TaskState.Completed);
        await _sut.DeleteAsync(id);
        Assert.False(_db.Context.Projects.Any(p => p.Id == id));
        Assert.False(_db.Context.Tasks.Any(t => t.ProjectId == id));
    }

    [Fact]
    public async Task Delete_NotFound_Throws()
        => await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(42));

    [Fact]
    public void ProjectRequest_EndBeforeStart_IsInvalid()
    {
        var request = new ProjectRequest { Name = "Valido", StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 4, 1) };
        var results = request.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(request)).ToList();
        Assert.Single(results);
    }
}
