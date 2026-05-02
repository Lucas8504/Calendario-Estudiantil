using System.Collections.ObjectModel;
using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para el grid de horarios (único ViewModel de schedule).
/// Implementa IDatabaseService para acceso directo a datos (evita N+1 queries).
/// </summary>
public partial class ScheduleGridViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;

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

    // Hora actual para el indicador
    [ObservableProperty]
    private TimeSpan _currentTime;

    [ObservableProperty]
    private bool _isToday;

    public ScheduleGridViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeWeek();
        UpdateCurrentTime();
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
            if (!IsToday || CurrentTime.TotalHours < MinHour || CurrentTime.TotalHours >= MaxHour)
                return -1;
            return (CurrentTime.TotalHours - MinHour) / (MaxHour - MinHour);
        }
    }

    [RelayCommand]
    public async Task LoadGridAsync()
    {
        IsLoading = true;
        UpdateCurrentTime();

        try
        {
            // 1. Cargar TODOS los horarios directamente (evita N+1 queries)
            var allSchedules = await LoadAllSchedulesOptimizedAsync();

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
    /// Carga todos los horarios con una sola query compleja (NO hace N+1).
    /// </summary>
    private async Task<List<ScheduleItemHelper>> LoadAllSchedulesOptimizedAsync()
    {
        // Cargar horarios y materias en queries separadas, luego combinar en memoria
        var schedules = await _databaseService.GetAllSchedulesAsync();
        if (!schedules.Any())
            return new List<ScheduleItemHelper>();

        var subjectIds = schedules.Select(s => s.SubjectId).Distinct().ToList();
        var subjects = await _databaseService.GetSubjectsByIdsAsync(subjectIds);

        var subjectDict = subjects.ToDictionary(s => s.Id);
        
        var result = new List<ScheduleItemHelper>();
        foreach (var schedule in schedules)
        {
            if (subjectDict.TryGetValue(schedule.SubjectId, out var subject))
            {
                result.Add(new ScheduleItemHelper { Subject = subject, Schedule = schedule });
            }
        }

        return result;
    }

    /// <summary>
    /// Calcula las horas mínimo y máximo basándose en los horarios ocupados.
    /// </summary>
    private void CalculateDynamicHours(List<ScheduleItemHelper> allSchedules)
    {
        var earliestStart = allSchedules.Min(s => s.Schedule.StartTime);
        var latestEnd = allSchedules.Max(s => s.Schedule.EndTime);

        var calculatedMinHour = earliestStart.Hours;
        var calculatedMaxHour = latestEnd.Hours + 1;

        MinHour = Math.Max(calculatedMinHour, 6);
        MaxHour = Math.Min(calculatedMaxHour, 23);
        TotalHours = MaxHour - MinHour;
    }

    /// <summary>
    /// Crea los headers de días basándose en los días con clases.
    /// </summary>
    private ObservableCollection<DayHeader> CreateDayHeaders(List<DiaSemana> daysWithClasses)
    {
        var headers = new ObservableCollection<DayHeader>();

        foreach (var day in daysWithClasses)
        {
            var dayIndex = (int)day - 1;
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