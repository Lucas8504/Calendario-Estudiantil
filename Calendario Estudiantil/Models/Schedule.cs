using SQLite;

namespace Calendario_Estudiantil.Models;

/// <summary>
/// Horario semanal de una materia.
/// </summary>
public class Schedule
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int SubjectId { get; set; }

    /// <summary>
    /// Día de la semana (1=Lunes, 7=domingo).
    /// </summary>
    public DiaSemana Day { get; set; }

    /// <summary>
    /// Hora de inicio (ej: 08:00).
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Hora de fin (ej: 10:00).
    /// </summary>
    public TimeSpan EndTime { get; set; }
}