using System.Collections.ObjectModel;
using Calendario_Estudiantil.Configuration;
using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Models.Ui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para la vista diaria de horarios con cards.
/// </summary>
public partial class ScheduleDayViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<ScheduleCard> _cards = new();

    [ObservableProperty]
    private ObservableCollection<string> _dayTabs = new();

    [ObservableProperty]
    private string _selectedDay = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasNoClasses;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _hasNotifications;

    // Todas las cards sin filtrar (para búsqueda)
    private List<ScheduleCard> _allCards = new();

    // Días de la semana en orden
    private static readonly string[] WeekDays = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

    public ScheduleDayViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        InitializeDayTabs();
    }

    private void InitializeDayTabs()
    {
        DayTabs = new ObservableCollection<string>(WeekDays);
        // Seleccionar el día actual por defecto
        var todayIndex = (int)DateTime.Today.DayOfWeek - 1;
        if (todayIndex < 0) todayIndex = 6; // Domingo = 6
        SelectedDay = WeekDays[todayIndex];
    }

    [RelayCommand]
    public async Task LoadDayAsync()
    {
        if (string.IsNullOrEmpty(SelectedDay)) return;

        IsLoading = true;
        HasNoClasses = false;

        try
        {
            var dayOfWeek = GetDiaSemana(SelectedDay);
            var allSchedules = await LoadAllSchedulesOptimizedAsync();
            
            // Filtrar por el día seleccionado
            var daySchedules = allSchedules
                .Where(s => s.Schedule.Day == dayOfWeek)
                .OrderBy(s => s.Schedule.StartTime)
                .ToList();

            _allCards = daySchedules
                .Select(s => new ScheduleCard 
                { 
                    Subject = s.Subject, 
                    Schedule = s.Schedule 
                })
                .ToList();

            ApplySearchFilter();
            
            // Verificar notificaciones (tareas pendientes para hoy)
            await CheckNotificationsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Verifica si hay notificaciones pendientes.
    /// </summary>
    private async Task CheckNotificationsAsync()
    {
        try
        {
            // Verificar si hay tareas pendientes para hoy
            var pendingTasks = await _databaseService.GetPendingTasksAsync();
            HasNotifications = pendingTasks.Any(t => t.DueDate.Date == DateTime.Today);
        }
        catch
        {
            HasNotifications = false;
        }
    }

    /// <summary>
    /// Aplica el filtro de búsqueda sobre las cards del día seleccionado.
    /// </summary>
    private void ApplySearchFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allCards
            : _allCards.Where(c => 
                c.SubjectName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                c.TimeRange.Contains(SearchQuery))
              .ToList();

        Cards = new ObservableCollection<ScheduleCard>(filtered);
        HasNoClasses = !Cards.Any();
    }

    /// <summary>
    /// Busca horarios por nombre de materia o horario.
    /// </summary>
    [RelayCommand]
    public void Search(string query)
    {
        SearchQuery = query;
        ApplySearchFilter();
    }

    /// <summary>
    /// Limpia el filtro de búsqueda.
    /// </summary>
    [RelayCommand]
    public void ClearSearch()
    {
        SearchQuery = string.Empty;
        ApplySearchFilter();
    }

    private DiaSemana GetDiaSemana(string dayName)
    {
        return dayName switch
        {
            "Lunes" => DiaSemana.Lunes,
            "Martes" => DiaSemana.Martes,
            "Miércoles" => DiaSemana.Miércoles,
            "Jueves" => DiaSemana.Jueves,
            "Viernes" => DiaSemana.Viernes,
            "Sábado" => DiaSemana.Sábado,
            "Domingo" => DiaSemana.Domingo,
            _ => DiaSemana.Lunes
        };
    }

    /// <summary>
    /// Carga todos los horarios con 2 queries (evita N+1).
    /// </summary>
    private async Task<List<ScheduleItemHelper>> LoadAllSchedulesOptimizedAsync()
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

    [RelayCommand]
    public async Task SelectDayAsync(string day)
    {
        SelectedDay = day;
        await LoadDayAsync();
    }
}
