using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NexusPlan.Application.Dtos;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Tests.Integration;

public class ApiEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<ProjectResponse> CreateProjectAsync(string name = "Proyecto integración")
    {
        var res = await _client.PostAsJsonAsync("/api/projects",
            new { name, description = "Desc", startDate = "2026-01-01", endDate = "2026-12-31" }, Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<ProjectResponse>(Json))!;
    }

    private async Task<TaskResponse> CreateTaskAsync(int projectId, string dueDate = "2099-01-01", string priority = "High")
    {
        var res = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Tarea integración", priority, dueDate, projectId }, Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return (await res.Content.ReadFromJsonAsync<TaskResponse>(Json))!;
    }

    [Fact]
    public async Task Swagger_IsAvailable()
    {
        var res = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("/api/projects/{id}/progress", await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Health_IsOk()
        => Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);

    [Fact]
    public async Task CreateProject_Invalid_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/api/projects",
            new { name = "", startDate = "2026-01-01", endDate = "2026-02-01" }, Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Contains("Name", await res.Content.ReadAsStringAsync());

        res = await _client.PostAsJsonAsync("/api/projects",
            new { name = "Fechas mal", startDate = "2026-05-01", endDate = "2026-01-01" }, Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Contains("EndDate", await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task CreateTask_InvalidEnum_Returns400()
    {
        var project = await CreateProjectAsync();
        var res = await _client.PostAsJsonAsync("/api/tasks",
            new { title = "Tarea", priority = "Urgentisima", dueDate = "2026-01-01", projectId = project.Id }, Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetProject_NotFound_Returns404ProblemDetails()
    {
        var res = await _client.GetAsync("/api/projects/99999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Contains("99999", await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task FullFlow_StateChange_Progress_Overdue_And_DeleteRule()
    {
        var project = await CreateProjectAsync();
        var t1 = await CreateTaskAsync(project.Id);
        var t2 = await CreateTaskAsync(project.Id, dueDate: "2000-01-01", priority: "Low");

        // Tareas del proyecto + filtros
        var all = await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{project.Id}/tasks", Json);
        Assert.Equal(2, all!.Count);
        var low = await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{project.Id}/tasks?priority=Low&state=Pending", Json);
        Assert.Single(low!);

        // Vencidas
        var overdue = await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/tasks/overdue?projectId={project.Id}", Json);
        Assert.Equal(t2.Id, Assert.Single(overdue!).Id);

        // No se puede eliminar con tareas pendientes
        var del = await _client.DeleteAsync($"/api/projects/{project.Id}");
        Assert.Equal(HttpStatusCode.Conflict, del.StatusCode);

        // Cambiar estado
        var patch = await _client.PatchAsJsonAsync($"/api/tasks/{t1.Id}/state", new { state = "Completed" }, Json);
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var progress = await _client.GetFromJsonAsync<ProjectProgressResponse>($"/api/projects/{project.Id}/progress", Json);
        Assert.Equal(50m, progress!.ProgressPercentage);

        await _client.PatchAsJsonAsync($"/api/tasks/{t2.Id}/state", new { state = TaskState.Completed }, Json);
        progress = await _client.GetFromJsonAsync<ProjectProgressResponse>($"/api/projects/{project.Id}/progress", Json);
        Assert.Equal(100m, progress!.ProgressPercentage);

        // Ahora sí se puede eliminar
        del = await _client.DeleteAsync($"/api/projects/{project.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/tasks/{t1.Id}")).StatusCode);
    }

    [Fact]
    public async Task UpdateAndDeleteTask()
    {
        var project = await CreateProjectAsync();
        var task = await CreateTaskAsync(project.Id);

        var put = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}",
            new { title = "Actualizada", state = "InProgress", priority = "Medium", dueDate = "2099-02-01", projectId = project.Id }, Json);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var updated = await put.Content.ReadFromJsonAsync<TaskResponse>(Json);
        Assert.Equal("Actualizada", updated!.Title);
        Assert.Equal(TaskState.InProgress, updated.State);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/tasks/{task.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/tasks/{task.Id}")).StatusCode);
    }
}
