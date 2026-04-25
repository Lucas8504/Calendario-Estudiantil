using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// Representa una celda del grid de horarios (aplanada).
/// </summary>
public class ScheduleCell
{
    public int Hour { get; set; }
    public int DayIndex { get; set; }
    public string DayName { get; set; } = "";
    public DateTime Date { get; set; }
    
    public Subject? Subject { get; set; }
    public Schedule? Schedule { get; set; }
    public bool HasClass => Subject != null;
    
    public string ColorHex => Subject?.ColorHex ?? "#FAFAFA";
    public string HourDisplay => $"{Hour:00}:00";
}