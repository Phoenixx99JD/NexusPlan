using System.ComponentModel.DataAnnotations;

namespace NexusPlan.Application.Dtos;

/// <summary>Datos para crear o actualizar un proyecto.</summary>
public class ProjectRequest : IValidatableObject
{
    /// <example>Migración a la nube</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    /// <example>Migrar los servicios on-premise a Azure.</example>
    [StringLength(1000, ErrorMessage = "La descripción no puede superar 1000 caracteres.")]
    public string? Description { get; set; }

    /// <example>2026-01-01</example>
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime? StartDate { get; set; }

    /// <example>2026-06-30</example>
    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    public DateTime? EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult(
                "La fecha de finalización no puede ser anterior a la fecha de inicio.",
                [nameof(EndDate)]);
        }
    }
}

/// <summary>Representación de un proyecto.</summary>
public record ProjectResponse(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    int TotalTasks,
    int CompletedTasks,
    decimal ProgressPercentage);

/// <summary>Avance de un proyecto calculado a partir de sus tareas completadas.</summary>
public record ProjectProgressResponse(
    int ProjectId,
    string ProjectName,
    int TotalTasks,
    int CompletedTasks,
    int InProgressTasks,
    int PendingTasks,
    decimal ProgressPercentage);
