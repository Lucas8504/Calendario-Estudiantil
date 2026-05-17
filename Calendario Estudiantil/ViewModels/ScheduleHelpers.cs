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
    
    /// <summary>
    /// Duración de la clase en formato legible (ej: "1h 30m").
    /// </summary>
    public string Duration
    {
        get
        {
            var duration = Schedule.EndTime - Schedule.StartTime;
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            
            if (hours > 0 && minutes > 0)
                return $"{hours}h {minutes}m";
            if (hours > 0)
                return $"{hours}h";
            return $"{minutes}m";
        }
    }
    
    /// <summary>
    /// Indica si la clase está en curso ahora (solo para el día actual).
    /// </summary>
    public bool IsHappeningNow
    {
        get
        {
            var today = DateTime.Today;
            var now = DateTime.Now.TimeOfDay;
            var classDay = GetDateTimeFromDay(Schedule.Day);
            
            // Solo verificar si es hoy
            if (classDay.Date != today)
                return false;
            
            return now >= Schedule.StartTime && now <= Schedule.EndTime;
        }
    }
    
    private DateTime GetDateTimeFromDay(DiaSemana day)
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = today.AddDays(-diff);
        
        return day switch
        {
            DiaSemana.Lunes => monday,
            DiaSemana.Martes => monday.AddDays(1),
            DiaSemana.Miércoles => monday.AddDays(2),
            DiaSemana.Jueves => monday.AddDays(3),
            DiaSemana.Viernes => monday.AddDays(4),
            DiaSemana.Sábado => monday.AddDays(5),
            DiaSemana.Domingo => monday.AddDays(6),
            _ => today
        };
    }
}
