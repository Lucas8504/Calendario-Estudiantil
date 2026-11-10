namespace Calendario_Estudiantil.Models;

/// <summary>
/// Comandos inmutables que representa el asistente de voz para operar sobre el dominio de la app.
/// Cada registro corresponde a una función (tool) disponible para Gemini — el schema de salida de
/// la IA ES ESTE CONTRATO. Validar estrictamente antes de ejecutar.
/// </summary>

// ---- Tareas ----
public record AddTaskCommand(
    string Title,
    string? Description,
    int? SubjectId,
    string? SubjectName,
    DateTime DueDate,
    int NotificationHours);

public record UpdateTaskCommand(
    int TaskId,
    string? Title,
    string? Description,
    int? SubjectId,
    string? SubjectName,
    DateTime? DueDate,
    ItemStatus? Status,
    int? NotificationHours);

public record DeleteTaskCommand(int TaskId);

// ---- Materias ----
public record AddSubjectCommand(string Name, string? ColorHex);

public record UpdateSubjectCommand(int SubjectId, string? Name, string? ColorHex);

public record DeleteSubjectCommand(int SubjectId);

// ---- Horarios ----
public record AddScheduleCommand(int SubjectId, string Day, string StartTime, string EndTime);

public record UpdateScheduleCommand(int ScheduleId, int? SubjectId, string? Day, string? StartTime, string? EndTime);

public record DeleteScheduleCommand(int ScheduleId);

// ---- Consultas (lookup interno, no visible en la UI) ----
public record FindSubjectCommand(string Name);
