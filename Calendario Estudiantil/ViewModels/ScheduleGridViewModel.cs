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

    // Marcas de tiempo para la columna "regla" (inicio/fin de materias)
    public List<TimeMark> TimeMarks { get; set; } = new();

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
                TimeMarks = new List<TimeMark>();
                return;
            }

            // 2. Calcular marcas de tiempo dinámicas (inicio/fin de materias)
            TimeMarks = CalculateTimeMarks(allSchedules);

            // Actualizar min/max basados en las marcas para compatibilidad
            MinHour = (int)TimeMarks.First().Time.TotalHours;
            MaxHour = (int)TimeMarks.Last().Time.TotalHours + 1;

            // 3. Crear headers de días
            var daysWithClasses = allSchedules
                .Select(s => s.Schedule.Day)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            DayHeaders = CreateDayHeaders(daysWithClasses);

            // 4. Crear celdas del grid
            Cells = CreateScheduleCells(allSchedules, daysWithClasses, TimeMarks);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Carga horarios filtrando por curso activo si existe.
    /// Si no hay curso activo o el curso activo no tiene datos, hace fallback
    /// a la carga completa (comportamiento anterior).
    /// </summary>
    private async Task<List<ScheduleItemHelper>> LoadAllSchedulesOptimizedAsync()
    {
        async Task<List<ScheduleItemHelper>> LoadAllFallback()
        {
            var schedules = await _databaseService.GetAllSchedulesAsync();
            if (!schedules.Any()) return new List<ScheduleItemHelper>();

            var subjectIds = schedules.Select(s => s.SubjectId).Distinct().ToList();
            var subjects = await _databaseService.GetSubjectsByIdsAsync(subjectIds);
            var subjectDict = subjects.ToDictionary(s => s.Id);

            var result = new List<ScheduleItemHelper>();
            foreach (var schedule in schedules)
            {
                if (subjectDict.TryGetValue(schedule.SubjectId, out var subject))
                    result.Add(new ScheduleItemHelper { Subject = subject, Schedule = schedule });
            }
            return result;
        }

        // 1. Obtener el curso activo
        var activeCourseId = await _databaseService.GetActiveCourseIdAsync();
        if (activeCourseId <= 0)
            return await LoadAllFallback();

        // 2. Cargar materias del curso activo
        var subjects = await _databaseService.GetSubjectsByCourseAsync(activeCourseId);
        if (!subjects.Any())
            return await LoadAllFallback();

        var subjectDict = subjects.ToDictionary(s => s.Id);
        var subjectIds = subjects.Select(s => s.Id).ToList();

        // 3. Cargar horarios SOLO de esas materias
        var schedulesDict = await _databaseService.GetSchedulesForSubjectsAsync(subjectIds);

        // 4. Combinar en memoria
        var result = new List<ScheduleItemHelper>();
        foreach (var (subjectId, scheduleList) in schedulesDict)
        {
            if (!subjectDict.TryGetValue(subjectId, out var subject)) continue;
            foreach (var schedule in scheduleList)
            {
                result.Add(new ScheduleItemHelper { Subject = subject, Schedule = schedule });
            }
        }

        // Si por alguna razón el curso activo no tiene horarios, fallback
        if (!result.Any())
            return await LoadAllFallback();

        return result;
    }

    /// <summary>
    /// Calcula las marcas de tiempo para la columna "regla".
    /// Usa el rango configurado (MinHour..MaxHour) como base y agrega horas
    /// de inicio/fin de materias. Así la grilla siempre muestra el rango completo.
    /// </summary>
    private List<TimeMark> CalculateTimeMarks(List<ScheduleItemHelper> allSchedules)
    {
        var marks = new HashSet<TimeSpan>();

        // Rango fijo desde configuración (ej: 7:00..23:00)
        var rangeMin = TimeSpan.FromHours(AppConstants.ScheduleGrid.MinHour);
        var rangeMax = TimeSpan.FromHours(AppConstants.ScheduleGrid.MaxHour);

        // Agregar los bordes del rango configurado
        marks.Add(rangeMin);
        marks.Add(rangeMax);

        // Agregar horas enteras del rango completo
        for (int h = AppConstants.ScheduleGrid.MinHour; h <= AppConstants.ScheduleGrid.MaxHour; h++)
        {
            marks.Add(TimeSpan.FromHours(h));
        }

        // Agregar inicios y fines de materias (incluye :00, :30, :45, etc.)
        foreach (var s in allSchedules)
        {
            marks.Add(s.Schedule.StartTime);
            marks.Add(s.Schedule.EndTime);
        }

        return marks
            .Where(t => t >= rangeMin && t <= rangeMax)
            .Distinct()
            .OrderBy(t => t)
            .Select(t => new TimeMark 
            { 
                Time = t, 
                Label = t.Minutes == 0 ? $"{t.Hours:D2}:00" : $"{t.Hours:D2}:{t.Minutes:D2}",
                IsWholeHour = t.Minutes == 0
            })
            .ToList();
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
    /// Crea las celdas del grid de horarios con RowSpan pre-calculado.
    /// </summary>
    private ObservableCollection<ScheduleCell> CreateScheduleCells(
        List<ScheduleItemHelper> allSchedules, 
        List<DiaSemana> daysWithClasses,
        List<TimeMark> timeMarks)
    {
        var cells = new ObservableCollection<ScheduleCell>();
        var startedSchedules = new HashSet<(int dayIndex, int scheduleId)>();

        // Pre-agrupar horarios por día para evitar scanear todo allSchedules en cada iteración
        var schedulesByDay = allSchedules
            .GroupBy(s => s.Schedule.Day)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int i = 0; i < timeMarks.Count - 1; i++)
        {
            var startMark = timeMarks[i];
            var endMark = timeMarks[i + 1];
            var hour = startMark.Time.Hours;

            foreach (var day in daysWithClasses)
            {
                var dayIndex = (int)day - 1;
                var dayDate = CurrentWeekStart.AddDays(dayIndex);

                // Buscar solo dentro del día correspondiente (evita scanear horarios de otros días)
                if (!schedulesByDay.TryGetValue(day, out var daySchedules)) continue;

                var matchingSchedule = daySchedules.FirstOrDefault(s => 
                    s.Schedule.StartTime <= startMark.Time && 
                    s.Schedule.EndTime >= endMark.Time);

                int rowSpan = 0;
                if (matchingSchedule != null)
                {
                    var key = (dayIndex, matchingSchedule.Schedule.Id);
                    if (!startedSchedules.Contains(key))
                    {
                        startedSchedules.Add(key);

                        // Pre-calcular RowSpan: cuántos intervalos cubre desde i
                        for (int j = i; j < timeMarks.Count - 1; j++)
                        {
                            if (timeMarks[j].Time >= matchingSchedule.Schedule.StartTime &&
                                timeMarks[j + 1].Time <= matchingSchedule.Schedule.EndTime)
                                rowSpan++;
                            else if (timeMarks[j].Time > matchingSchedule.Schedule.StartTime)
                                break;
                        }
                    }
                }
                rowSpan = Math.Max(1, rowSpan);

                cells.Add(new ScheduleCell
                {
                    Hour = hour,
                    DayIndex = dayIndex,
                    DayName = GetDayName(day),
                    Date = dayDate,
                    Subject = matchingSchedule?.Subject,
                    Schedule = matchingSchedule?.Schedule,
                    StartTime = startMark.Time,
                    EndTime = endMark.Time,
                    RowSpan = rowSpan
                });
            }
        }

        return cells;
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

/// <summary>
/// Representa una marca en la "regla" de horarios.
/// </summary>
public class TimeMark
{
    public TimeSpan Time { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsWholeHour { get; set; }
}