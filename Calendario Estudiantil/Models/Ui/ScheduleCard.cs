using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Models.Ui;

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
