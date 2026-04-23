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
    private ObservableCollection<ScheduleDisplay> _schedules = new();

    [ObservableProperty]
    private bool _isLoading;

    public ScheduleViewModel(ISubjectService subjectService, ICourseService courseService)
    {
        _subjectService = subjectService;
        _courseService = courseService;

        // Iniciar en semana actual (lunes)
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        CurrentWeekStart = today.AddDays(-diff);
    }

    [RelayCommand]
    public async Task LoadSchedulesAsync()
    {
        IsLoading = true;
        try
        {
            var activeCourse = await _courseService.GetActiveAsync();
            if (activeCourse == null)
            {
                Schedules.Clear();
                return;
            }

            var subjects = await _subjectService.GetByCourseAsync(activeCourse.Id);
            var list = new List<ScheduleDisplay>();

            foreach (var subject in subjects)
            {
                var schedules = await _subjectService.GetSchedulesAsync(subject.Id);
                foreach (var schedule in schedules)
                {
                    list.Add(new ScheduleDisplay { Subject = subject, Schedule = schedule });
                }
            }

            Schedules = new ObservableCollection<ScheduleDisplay>(list);
        }
        finally
        {
            IsLoading = false;
        }
    }

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