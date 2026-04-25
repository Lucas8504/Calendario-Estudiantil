using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para el calendario semanal.
/// </summary>
public partial class ScheduleViewModel : ObservableObject
{
    private readonly ISubjectService _subjectService;
    private readonly ICourseService _courseService;

    [ObservableProperty]
    private DateTime _currentWeekStart;

    [ObservableProperty]
    private ObservableCollection<DayColumn> _weekDays = new();

    [ObservableProperty]
    private TimeSpan _currentTime;

    [ObservableProperty]
    private bool _isToday;

    [ObservableProperty]
    private bool _isLoading;

    // Constants
    public static readonly int StartHour = 7;
    public static readonly int EndHour = 22;

    [ObservableProperty]
    private int _globalMinHour = 7;

    [ObservableProperty]
    private int _globalMaxHour = 22;

    public ScheduleViewModel(ISubjectService subjectService, ICourseService courseService)
    {
        _subjectService = subjectService;
        _courseService = courseService;

        // Iniciar en semana actual (lunes)
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        CurrentWeekStart = today.AddDays(-diff);

        UpdateCurrentTime();
    }

    /// <summary>
    /// Actualiza la hora actual para el indicador.
    /// </summary>
    public void UpdateCurrentTime()
    {
        CurrentTime = DateTime.Now.TimeOfDay;
        IsToday = DateTime.Today >= CurrentWeekStart && DateTime.Today < CurrentWeekStart.AddDays(7);
    }

    /// <summary>
    /// Calcula la posición del indicador de hora actual (0-1).
    /// </summary>
    public double CurrentTimePosition
    {
        get
        {
            if (!IsToday || CurrentTime.TotalHours < StartHour || CurrentTime.TotalHours >= EndHour)
                return -1;
            return (CurrentTime.TotalHours - StartHour) / (EndHour - StartHour);
        }
    }

    [RelayCommand]
    public async Task LoadSchedulesAsync()
    {
        IsLoading = true;
        UpdateCurrentTime();

        try
        {
            // Cargar todas las materias (sin carrera)
            var subjects = await _subjectService.GetAllAsync();

            // Cargar todos los horarios de las materias
            var allSchedules = new List<ScheduleDisplay>();
            foreach (var subject in subjects)
            {
                var schedules = await _subjectService.GetSchedulesAsync(subject.Id);
                foreach (var schedule in schedules)
                {
                    allSchedules.Add(new ScheduleDisplay { Subject = subject, Schedule = schedule });
                }
            }

            GenerateWeekGrid(allSchedules);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Genera las columnas de la semana con sus time slots.
    /// SIEMPRE genera 15 slots por día (7am-10pm) para alineación.
    /// </summary>
    private void GenerateWeekGrid(List<ScheduleDisplay> allSchedules)
    {
        var days = new ObservableCollection<DayColumn>();

        // Verificar qué días tienen clases
        var daysWithClasses = allSchedules
            .Select(s => s.Schedule.Day)
            .Distinct()
            .ToHashSet();

        // SIEMPRE usar rango fijo 7am-10pm (15 horas)
        int minHour = StartHour;  // 7
        int maxHour = EndHour;    // 22

        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DiaSemana)(i + 1);

            // Solo incluir días que tienen clases
            if (!daysWithClasses.Contains(dayOfWeek))
                continue;

            var date = CurrentWeekStart.AddDays(i);
            var daySchedules = allSchedules.Where(s => s.Schedule.Day == dayOfWeek).ToList();

            var slots = new ObservableCollection<TimeSlot>();

            // SIEMPRE generar 15 slots (7am a 10pm)
            for (int hour = minHour; hour < maxHour; hour++)
            {
                var slotStart = TimeSpan.FromHours(hour);
                var slotEnd = TimeSpan.FromHours(hour + 1);

                var matchingSchedule = daySchedules
                    .FirstOrDefault(s => s.Schedule.StartTime < slotEnd && s.Schedule.EndTime > slotStart);

                slots.Add(new TimeSlot
                {
                    Hour = hour,
                    Date = date,
                    Subject = matchingSchedule?.Subject,
                    Schedule = matchingSchedule?.Schedule,
                    HasClass = matchingSchedule != null
                });
            }

            days.Add(new DayColumn
            {
                Date = date,
                DayName = GetDayName(dayOfWeek),
                DayNumber = date.Day,
                IsToday = date.Date == DateTime.Today,
                TimeSlots = slots,
                TotalHours = maxHour - minHour  // 15
            });
        }

        WeekDays = days;
    }

    /// <summary>
    /// Genera una semana vacía (sin materias).
    /// </summary>
    private void GenerateEmptyWeek()
    {
        WeekDays = new ObservableCollection<DayColumn>();
    }

    /// <summary>
    /// Retorna true si no hay materias agregadas.
    /// </summary>
    public bool HasNoSubjects => WeekDays == null || WeekDays.Count == 0;

    private string GetDayName(DiaSemana day) => day switch
    {
        DiaSemana.Lunes => "Lun",
        DiaSemana.Martes => "Mar",
        DiaSemana.Miércoles => "Mié",
        DiaSemana.Jueves => "Jue",
        DiaSemana.Viernes => "Vie",
        DiaSemana.Sábado => "Sáb",
        DiaSemana.Domingo => "Dom",
        _ => ""
    };

    [RelayCommand]
    public async Task NextWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(7);
        await LoadSchedulesAsync();
    }

    [RelayCommand]
    public async Task PreviousWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        await LoadSchedulesAsync();
    }

    [RelayCommand]
    public async Task GoToTodayAsync()
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        CurrentWeekStart = today.AddDays(-diff);
        await LoadSchedulesAsync();
    }
}

/// <summary>
/// Helper para mostrar horarios en el grid.
/// </summary>
public class ScheduleDisplay
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
}

/// <summary>
/// Representa una columna (día) de la semana.
/// </summary>
public class DayColumn
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = "";
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
    public ObservableCollection<TimeSlot> TimeSlots { get; set; } = new();
    public int TotalHours { get; set; } = 15; // Default 7am-10pm
}

/// <summary>
/// Representa un slot de hora en el grid.
/// </summary>
public class TimeSlot
{
    public int Hour { get; set; }
    public DateTime Date { get; set; }
    public Subject? Subject { get; set; }
    public Schedule? Schedule { get; set; }
    public bool HasClass { get; set; }

    public string TimeDisplay => $"{Hour:00}:00";

    /// <summary>
    /// Cuántas horas dura la clase (para rowspan).
    /// </summary>
    public int DurationHours
    {
        get
        {
            if (Schedule == null) return 1;
            var duration = (Schedule.EndTime - Schedule.StartTime).TotalHours;
            return Math.Max(1, (int)Math.Ceiling(duration));
        }
    }
}