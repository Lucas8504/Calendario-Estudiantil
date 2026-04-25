using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para el grid de horarios optimizado.
/// </summary>
public partial class ScheduleGridViewModel : ObservableObject
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
    private string _weekLabel = "";

    // Rango global de horas
    [ObservableProperty]
    private int _minHour = 7;

    [ObservableProperty]
    private int _maxHour = 22;

    [ObservableProperty]
    private int _totalHours = 15;

    public ScheduleGridViewModel(ISubjectService subjectService)
    {
        _subjectService = subjectService;
        
        // Iniciar en semana actual (lunes)
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
            // 1. Cargar todas las materias con horarios
            var subjects = await _subjectService.GetAllAsync();
            
            // 2. Cargar horarios de todas las materias
            var allSchedules = new List<ScheduleItemHelper>();
            foreach (var subject in subjects)
            {
                var schedules = await _subjectService.GetSchedulesAsync(subject.Id);
                foreach (var schedule in schedules)
                {
                    allSchedules.Add(new ScheduleItemHelper { Subject = subject, Schedule = schedule });
                }
            }

            if (!allSchedules.Any())
            {
                // Sin materias - mostrar grid vacío
                DayHeaders = new ObservableCollection<DayHeader>();
                Cells = new ObservableCollection<ScheduleCell>();
                return;
            }

            // 3. Calcular rango GLOBAL de horas
            var globalMinHour = (int)allSchedules.Min(s => s.Schedule.StartTime.TotalHours);
            var globalMaxHour = (int)Math.Ceiling(allSchedules.Max(s => s.Schedule.EndTime.TotalHours));
            
            // Limitar al rango 7am-10pm
            if (globalMinHour < 7) globalMinHour = 7;
            if (globalMaxHour > 22) globalMaxHour = 22;
            if (globalMaxHour <= globalMinHour) globalMaxHour = globalMinHour + 1;
            
            MinHour = globalMinHour;
            MaxHour = globalMaxHour;
            TotalHours = globalMaxHour - globalMinHour;

            // 4. Encontrar días con clases
            var daysWithClasses = allSchedules
                .Select(s => s.Schedule.Day)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // 5. Crear headers de días
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
            DayHeaders = headers;

            // 6. Crear celdas aplanadas (grid row-major: todos los días para hora 7, luego hora 8, etc.)
            var cells = new ObservableCollection<ScheduleCell>();
            
            for (int hour = globalMinHour; hour < globalMaxHour; hour++)
            {
                foreach (var day in daysWithClasses)
                {
                    var dayIndex = (int)day - 1;
                    var dayDate = CurrentWeekStart.AddDays(dayIndex);
                    
                    // Buscar si hay materia en este día y hora
                    var matchingSchedule = allSchedules
                        .FirstOrDefault(s => s.Schedule.Day == day && 
                                          s.Schedule.StartTime <= TimeSpan.FromHours(hour + 1) && 
                                          s.Schedule.EndTime > TimeSpan.FromHours(hour));
                    
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
            
            Cells = cells;
        }
        finally
        {
            IsLoading = false;
        }
    }

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
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        CurrentWeekStart = today.AddDays(-diff);
        await LoadGridAsync();
    }
}

/// <summary>
/// Header de un día en el grid.
/// </summary>
public class DayHeader
{
    public int DayIndex { get; set; }
    public string DayName { get; set; } = "";
    public int DayNumber { get; set; }
    public bool IsToday { get; set; }
}

/// <summary>
/// Helper para mostrar horarios.
/// </summary>
public class ScheduleItemHelper
{
    public Subject Subject { get; set; } = null!;
    public Schedule Schedule { get; set; } = null!;
}