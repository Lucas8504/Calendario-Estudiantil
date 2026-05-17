using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// Representa una columna (día) de la semana en el grid.
/// </summary>
public class DayHeader
{
    public int DayIndex { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
}

/// <summary>
/// Representa una celda del grid de horarios.
/// </summary>
public class ScheduleCell
{
    public int Hour { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DayIndex { get; set; }
    public string DayName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public Subject? Subject { get; set; }
    public Schedule? Schedule { get; set; }
    
    public bool HasClass => Subject != null && Schedule != null;
    
    public string ColorHex => Subject?.ColorHex ?? "#FAFAFA";
    public string HourDisplay => $"{Hour:00}:00";

    /// <summary>
    /// Pre-calculado por CreateScheduleCells. Indica cuántas filas
    /// del grid ocupa esta celda (para materias multi-hora).
    /// </summary>
    public int RowSpan { get; set; } = 1;
}

/// <summary>
/// Helper para combinar Subject + Schedule (usado internamente).
/// </summary>
public class ScheduleItemHelper
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
}

/// <summary>
/// Representa una card de horario para la vista diaria.
/// </summary>
public class ScheduleCard
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
    public string TimeRange => $"{Schedule.StartTime:hh\\:mm} - {Schedule.EndTime:hh\\:mm}";
    public string SubjectName => Subject.Name;
    public string ColorHex => Subject.ColorHex;
}
