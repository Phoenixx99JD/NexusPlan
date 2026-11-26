using NexusPlan.Application.Dtos;
using NexusPlan.Application.Services;
using NexusPlan.Domain.Entities;
using NexusPlan.Domain.Enums;
using NexusPlan.Domain.Exceptions;

namespace NexusPlan.Tests.Unit;

public class TaskServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TaskService _sut;
    private readonly int _projectId;

    public TaskServiceTests()
    {
        _sut = new TaskService(_db.Tasks, _db.Projects, _db.Clock);
        var project = new Project { Name = "P", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31) };
        project.Tasks.Add(new TaskItem { Title = "Vencida pendiente", State = TaskState.Pending, Priority = TaskPriority.High, DueDate = new DateTime(2026, 6, 1) });
        project.Tasks.Add(new TaskItem { Title = "Vencida en progreso", State = TaskState.InProgress, Priority = TaskPriority.Low, DueDate = new DateTime(2026, 6, 10) });
        project.Tasks.Add(new TaskItem { Title = "Vencida completada", State = TaskState.Completed, Priority = TaskPriority.High, DueDate = new DateTime(2026, 5, 1) });
        project.Tasks.Add(new TaskItem { Title = "Futura", State = TaskState.Pending, Priority = TaskPriority.High, DueDate = new DateTime(2026, 8, 1) });
        _db.Context.Projects.Add(project);
        _db.Context.SaveChanges();
        _db.Context.ChangeTracker.Clear();
        _projectId = project.Id;
    }

    public void Dispose() => _db.Dispose();

    private TaskRequest NewRequest(int? projectId = null) => new()
    {
        Title = "Nueva tarea", Priority = TaskPriority.Critical, DueDate = new DateTime(2026, 9, 1), ProjectId = projectId ?? _projectId
    };

    [Fact]
    public async Task Create_DefaultsToPending()
    {
        var created = await _sut.CreateAsync(NewRequest());
        Assert.Equal(TaskState.Pending, created.State);
        Assert.Equal(_projectId, created.ProjectId);
    }

    [Fact]
    public async Task Create_WithUnknownProject_Throws()
        => await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(NewRequest(999)));

    [Fact]
    public async Task GetByProject_ReturnsAllTasks()
        => Assert.Equal(4, (await _sut.GetByProjectAsync(_projectId, null, null)).Count);

    [Fact]
    public async Task GetByProject_UnknownProject_Throws()
        => await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByProjectAsync(999, null, null));

    [Fact]
    public async Task Filter_ByStateAndPriority()
    {
        var result = await _sut.GetAllAsync(TaskState.Pending, TaskPriority.High, null);
        Assert.Equal(2, result.Count);
        Assert.All(result, t => { Assert.Equal(TaskState.Pending, t.State); Assert.Equal(TaskPriority.High, t.Priority); });

        Assert.Single(await _sut.GetAllAsync(null, TaskPriority.Low, null));
        Assert.Single(await _sut.GetByProjectAsync(_projectId, TaskState.Completed, null));
    }

    [Fact]
    public async Task Overdue_ReturnsOnlyPastDueAndNotCompleted()
    {
        var result = await _sut.GetOverdueAsync(null);
        Assert.Equal(new[] { "Vencida pendiente", "Vencida en progreso" }, result.Select(t => t.Title));
        Assert.All(result, t => Assert.True(t.IsOverdue));
    }

    [Fact]
    public async Task ChangeState_UpdatesState()
    {
        var task = (await _sut.GetOverdueAsync(null))[0];
        var updated = await _sut.ChangeStateAsync(task.Id, TaskState.Completed);
        Assert.Equal(TaskState.Completed, updated.State);
        Assert.False(updated.IsOverdue);
        Assert.Single(await _sut.GetOverdueAsync(null));
    }

    [Fact]
    public async Task ChangeState_InvalidValue_Throws()
        => await Assert.ThrowsAsync<ValidationException>(() => _sut.ChangeStateAsync(1, (TaskState)99));

    [Fact]
    public async Task ChangeState_NotFound_Throws()
        => await Assert.ThrowsAsync<NotFoundException>(() => _sut.ChangeStateAsync(999, TaskState.Completed));

    [Fact]
    public async Task Update_And_Delete()
    {
        var created = await _sut.CreateAsync(NewRequest());
        var req = NewRequest();
        req.Title = "Editada";
        req.State = TaskState.InProgress;
        var updated = await _sut.UpdateAsync(created.Id, req);
        Assert.Equal("Editada", updated.Title);
        Assert.Equal(TaskState.InProgress, updated.State);

        await _sut.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(created.Id));
    }
}
