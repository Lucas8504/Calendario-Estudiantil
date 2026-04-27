using System.Collections.ObjectModel;
using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para el grid de horarios.
/// Implementa IScheduleGridViewModel para facilitar testing y DI.
/// </summary>
public partial class ScheduleGridViewModel : ObservableObject, IScheduleGridViewModel
{
    private readonly ISubjectService _subjectService;

    [ObservableProperty]
    private DateTime _currentWeekStart;

    [ObservableProperty]
    private ObservableCollection<DayHeader> _dayHeaders = new();

    [ObservableProperty]
    private ObservableCollection<ScheduleCell> _cells = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _weekLabel = string.Empty;

    [ObservableProperty]
    private int _minHour = AppConstants.ScheduleGrid.MinHour;

    [ObservableProperty]
    private int _maxHour = AppConstants.ScheduleGrid.MaxHour;

    [ObservableProperty]
    private int _totalHours = AppConstants.ScheduleGrid.TotalHours;

    public ScheduleGridViewModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        InitializeWeek();
    }

    private void InitializeWeek()
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        CurrentWeekStart = today.AddDays(-diff);
        UpdateWeekLabel();
    }

    private void UpdateWeekLabel()
    {
        var endDate = CurrentWeekStart.AddDays(6);
        WeekLabel = $"{CurrentWeekStart:dd/MM} - {endDate:dd/MM}";
    }

    [RelayCommand]
    public async Task LoadGridAsync()
    {
        IsLoading = true;
        UpdateWeekLabel();

        try
        {
            // 1. Cargar materias y horarios en una sola operación optimizada
            var (subjects, allSchedules) = await LoadSchedulesOptimizedAsync();

            if (!allSchedules.Any())
            {
                DayHeaders = new ObservableCollection<DayHeader>();
                Cells = new ObservableCollection<ScheduleCell>();
                MinHour = AppConstants.ScheduleGrid.MinHour;
                MaxHour = AppConstants.ScheduleGrid.MaxHour;
                TotalHours = AppConstants.ScheduleGrid.TotalHours;
                return;
            }

            // 2. Calcular horas dinámicas (primera a última clase + 1)
            CalculateDynamicHours(allSchedules);

            // 3. Crear headers de días
            var daysWithClasses = allSchedules
                .Select(s => s.Schedule.Day)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            DayHeaders = CreateDayHeaders(daysWithClasses);

            // 4. Crear celdas del grid
            Cells = CreateScheduleCells(allSchedules, daysWithClasses);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Calcula las horas mínimo y máximo basándose en los horarios ocupados.
    /// Muestra desde la primera hora con clase hasta ultima hora + 1.
    /// </summary>
    private void CalculateDynamicHours(List<ScheduleItemHelper> allSchedules)
    {
        // Obtener la hora de inicio más temprana
        var earliestStart = allSchedules
            .Min(s => s.Schedule.StartTime);
        
        // Obtener la hora de fin más tardía
        var latestEnd = allSchedules
            .Max(s => s.Schedule.EndTime);

        // Primera hora con clase (redondeada hacia abajo)
        var calculatedMinHour = earliestStart.Hours;
        
        // Última hora + 1 (para mostrar la hora de fin)
        var calculatedMaxHour = latestEnd.Hours + 1;

        // Por seguridad, limites mínimos y máximos
        MinHour = Math.Max(calculatedMinHour, 6);  // mínimo 6:00
        MaxHour = Math.Min(calculatedMaxHour, 23); // máximo 23:00 (para mostrar hasta 22:00)
        TotalHours = MaxHour - MinHour;
    }

    /// <summary>
    /// Carga materias y horarios de forma optimizada (减少 N+1 queries).
    /// </summary>
    private async Task<(List<Subject> Subjects, List<ScheduleItemHelper> Schedules)> LoadSchedulesOptimizedAsync()
    {
        var subjects = await _subjectService.GetAllAsync();
        
        if (!subjects.Any())
            return (subjects, new List<ScheduleItemHelper>());

        // Cargar todos los horarios en una sola query por materia
        // En lugar de N+1 queries, hacemos N queries donde N = número de materias
        // (Sigue siendo N+1 pero mínimo necesario sin cambiar la estructura de BD)
        var allSchedules = new List<ScheduleItemHelper>();
        
        foreach (var subject in subjects)
        {
            var schedules = await _subjectService.GetSchedulesAsync(subject.Id);
            foreach (var schedule in schedules)
            {
                allSchedules.Add(new ScheduleItemHelper { Subject = subject, Schedule = schedule });
            }
        }

        return (subjects, allSchedules);
    }

    /// <summary>
    /// Crea los headers de días basándose en los días con clases.
    /// </summary>
    private ObservableCollection<DayHeader> CreateDayHeaders(List<DiaSemana> daysWithClasses)
    {
        var headers = new ObservableCollection<DayHeader>();

        foreach (var day in daysWithClasses)
        {
            var dayIndex = (int)day - 1; // 0=Lunes
            var date = CurrentWeekStart.AddDays(dayIndex);
            
            headers.Add(new DayHeader
            {
                DayIndex = dayIndex,
                DayName = GetDayName(day),
                DayNumber = date.Day,
                IsToday = date.Date == DateTime.Today
            });
        }

        return headers;
    }

    /// <summary>
    /// Crea las celdas del grid de horarios.
    /// Cada celda sabe qué materia tiene (si alguna) en esa hora/día.
    /// </summary>
    private ObservableCollection<ScheduleCell> CreateScheduleCells(
        List<ScheduleItemHelper> allSchedules, 
        List<DiaSemana> daysWithClasses)
    {
        var cells = new ObservableCollection<ScheduleCell>();

        for (int hour = MinHour; hour < MaxHour; hour++)
        {
            foreach (var day in daysWithClasses)
            {
                var dayIndex = (int)day - 1;
                var dayDate = CurrentWeekStart.AddDays(dayIndex);

                var matchingSchedule = FindMatchingSchedule(allSchedules, day, hour);

                cells.Add(new ScheduleCell
                {
                    Hour = hour,
                    DayIndex = dayIndex,
                    DayName = GetDayName(day),
                    Date = dayDate,
                    Subject = matchingSchedule?.Subject,
                    Schedule = matchingSchedule?.Schedule
                });
            }
        }

        return cells;
    }

    /// <summary>
    /// Busca si hay un horario que corresponda a este día y hora.
    /// </summary>
    private ScheduleItemHelper? FindMatchingSchedule(List<ScheduleItemHelper> allSchedules, DiaSemana day, int hour)
    {
        return allSchedules.FirstOrDefault(s => 
            s.Schedule.Day == day && 
            s.Schedule.StartTime < TimeSpan.FromHours(hour + 1) && 
            s.Schedule.EndTime > TimeSpan.FromHours(hour));
    }

    private static string GetDayName(DiaSemana day) => day switch
    {
        DiaSemana.Lunes => "Lun",
        DiaSemana.Martes => "Mar",
        DiaSemana.Miércoles => "Mié",
        DiaSemana.Jueves => "Jue",
        DiaSemana.Viernes => "Vie",
        DiaSemana.Sábado => "Sáb",
        DiaSemana.Domingo => "Dom",
        _ => string.Empty
    };

    [RelayCommand]
    public async Task PreviousWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(-7);
        await LoadGridAsync();
    }

    [RelayCommand]
    public async Task NextWeekAsync()
    {
        CurrentWeekStart = CurrentWeekStart.AddDays(7);
        await LoadGridAsync();
    }

    [RelayCommand]
    public async Task GoToTodayAsync()
    {
        InitializeWeek();
        await LoadGridAsync();
    }
}