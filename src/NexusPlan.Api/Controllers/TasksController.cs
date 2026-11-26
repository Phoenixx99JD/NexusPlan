using Microsoft.AspNetCore.Mvc;
using NexusPlan.Application.Dtos;
using NexusPlan.Application.Interfaces;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Api.Controllers;

/// <summary>Gestión de tareas.</summary>
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController(ITaskService tasks) : ControllerBase
{
    /// <summary>Lista tareas, con filtros opcionales por estado, prioridad y proyecto.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TaskState? state, [FromQuery] TaskPriority? priority, [FromQuery] int? projectId, CancellationToken ct)
        => Ok(await tasks.GetAllAsync(state, priority, projectId, ct));

    /// <summary>Consulta las tareas vencidas (fecha límite superada) que no están completadas.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue([FromQuery] int? projectId, CancellationToken ct)
        => Ok(await tasks.GetOverdueAsync(projectId, ct));

    /// <summary>Obtiene una tarea por su ID.</summary>
    [HttpGet("{id:int}", Name = nameof(GetTaskById))]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int id, CancellationToken ct) => Ok(await tasks.GetByIdAsync(id, ct));

    /// <summary>Crea una tarea dentro de un proyecto existente.</summary>
    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] TaskRequest request, CancellationToken ct)
    {
        var created = await tasks.CreateAsync(request, ct);
        return CreatedAtRoute(nameof(GetTaskById), new { id = created.Id }, created);
    }

    /// <summary>Actualiza una tarea.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] TaskRequest request, CancellationToken ct)
        => Ok(await tasks.UpdateAsync(id, request, ct));

    /// <summary>Cambia el estado de una tarea (Pending, InProgress, Completed).</summary>
    [HttpPatch("{id:int}/state")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeState(int id, [FromBody] ChangeTaskStateRequest request, CancellationToken ct)
        => Ok(await tasks.ChangeStateAsync(id, request.State!.Value, ct));

    /// <summary>Elimina una tarea.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await tasks.DeleteAsync(id, ct);
        return NoContent();
    }
}
