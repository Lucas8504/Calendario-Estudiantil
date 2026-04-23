using SQLite;

namespace Calendario_Estudiantil.Models;

/// <summary>
/// Estados posibles de una tarea.
/// </summary>
public enum ItemStatus
{
    Pending,
    InProgress,
    Completed,
    Expired
}

/// <summary>
/// Tarea asociada a una materia.
/// </summary>
public class TaskItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int SubjectId { get; set; }

    [NotNull]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de vencimiento.
    /// </summary>
    public DateTime DueDate { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Pending;

    /// <summary>
    /// Anticipación de notificación en horas.
    /// Si es menor a 12, mostrar warning rojo.
    /// </summary>
    public int NotificationHours { get; set; } = 24;
}