using System.ComponentModel.DataAnnotations;
using NexusPlan.Domain.Enums;

namespace NexusPlan.Application.Dtos;

/// <summary>Datos para crear o actualizar una tarea.</summary>
public class TaskRequest
{
    /// <example>Configurar base de datos</example>
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <example>Crear la instancia de Azure SQL y aplicar migraciones.</example>
    [StringLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres.")]
    public string? Description { get; set; }

    /// <summary>Estado inicial (Pending, InProgress, Completed). Por defecto Pending.</summary>
    [EnumDataType(typeof(TaskState), ErrorMessage = "Estado inválido. Valores: Pending, InProgress, Completed.")]
    public TaskState State { get; set; } = TaskState.Pending;

    /// <summary>Prioridad (Low, Medium, High, Critical).</summary>
    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [EnumDataType(typeof(TaskPriority), ErrorMessage = "Prioridad inválida. Valores: Low, Medium, High, Critical.")]
    public TaskPriority? Priority { get; set; }

    /// <example>2026-03-15</example>
    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    public DateTime? DueDate { get; set; }

    /// <example>1</example>
    [Required(ErrorMessage = "El ID del proyecto es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del proyecto debe ser mayor que 0.")]
    public int? ProjectId { get; set; }
}

/// <summary>Datos para cambiar el estado de una tarea.</summary>
public class ChangeTaskStateRequest
{
    /// <example>InProgress</example>
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [EnumDataType(typeof(TaskState), ErrorMessage = "Estado inválido. Valores: Pending, InProgress, Completed.")]
    public TaskState? State { get; set; }
}

/// <summary>Representación de una tarea.</summary>
public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    TaskState State,
    TaskPriority Priority,
    DateTime DueDate,
    int ProjectId,
    bool IsOverdue);
