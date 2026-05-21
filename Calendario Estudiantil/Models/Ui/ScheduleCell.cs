using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Models.Ui;

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
