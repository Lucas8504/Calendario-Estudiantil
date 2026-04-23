using SQLite;

namespace Calendario_Estudiantil.Models;

/// <summary>
/// Subtarea dentro de una tarea.
/// </summary>
public class Subtask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int TaskItemId { get; set; }

    [NotNull]
    public string Title { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}