using Microsoft.AspNetCore.Mvc;
using NexusPlan.Application.Dtos;
using NexusPlan.Application.Interfaces;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Api.Controllers;

/// <summary>Gestión de proyectos.</summary>
[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public class ProjectsController(IProjectService projects, ITaskService tasks) : ControllerBase
{
    /// <summary>Lista todos los proyectos con su porcentaje de avance.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProjectResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await projects.GetAllAsync(ct));

    /// <summary>Obtiene un proyecto por su ID.</summary>
    [HttpGet("{id:int}", Name = nameof(GetProjectById))]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(int id, CancellationToken ct) => Ok(await projects.GetByIdAsync(id, ct));

    /// <summary>Crea un proyecto.</summary>
    [HttpPost]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProjectRequest request, CancellationToken ct)
    {
        var created = await projects.CreateAsync(request, ct);
        return CreatedAtRoute(nameof(GetProjectById), new { id = created.Id }, created);
    }

    /// <summary>Actualiza un proyecto.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectRequest request, CancellationToken ct)
        => Ok(await projects.UpdateAsync(id, request, ct));

    /// <summary>Elimina un proyecto. Falla con 409 si tiene tareas pendientes o en progreso.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await projects.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Consulta las tareas de un proyecto, con filtros opcionales por estado y prioridad.</summary>
    [HttpGet("{id:int}/tasks")]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTasks(int id, [FromQuery] TaskState? state, [FromQuery] TaskPriority? priority, CancellationToken ct)
        => Ok(await tasks.GetByProjectAsync(id, state, priority, ct));

    /// <summary>Calcula el porcentaje de avance del proyecto según sus tareas completadas.</summary>
    [HttpGet("{id:int}/progress")]
    [ProducesResponseType<ProjectProgressResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(int id, CancellationToken ct) => Ok(await projects.GetProgressAsync(id, ct));
}
