using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Calendario_Estudiantil.Helpers;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para gestión de tareas.
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly ISubjectService? _subjectService;
    private Dictionary<int, Subject> _subjectsDict = new();

    [ObservableProperty]
    private ObservableCollection<TaskDisplay> _tasks = new();

    [ObservableProperty]
    private string _filterMode = "Todos"; // Todos | Hoy | Pendientes

    [ObservableProperty]
    private bool _isLoading;



    public TasksViewModel(ITaskService taskService, ISubjectService? subjectService = null)
    {
        _taskService = taskService;
        _subjectService = subjectService;
    }

    public async Task LoadSubjectsAsync()
    {
        if (_subjectService == null) return;
        var subjects = await _subjectService.GetAllAsync();
        _subjectsDict = subjects.ToDictionary(s => s.Id);
    }

    [RelayCommand]
    public async Task LoadTasksAsync()
    {
        IsLoading = true;
        try
        {
            List<TaskItem> list = FilterMode switch
            {
                "Hoy" => await _taskService.GetForTodayAsync(),
                "Pendientes" => await _taskService.GetPendingAsync(),
                _ => await _taskService.GetAllAsync()
            };

            var display = list.Select(t => new TaskDisplay
            {
                TaskItem = t,
                IsShortAnticipation = _taskService.IsShortAnticipation(t.NotificationHours),
                SubjectName = _subjectsDict.TryGetValue(t.SubjectId, out var s) ? s.Name : null,
                SubjectColor = _subjectsDict.TryGetValue(t.SubjectId, out var s2) ? s2.ColorHex : "#6B7280"
            }).ToList();

            Tasks.ReplaceWith(display);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CompleteTaskAsync(TaskDisplay? taskDisplay)
    {
        if (taskDisplay?.TaskItem == null) return;
        await _taskService.UpdateStatusAsync(taskDisplay.TaskItem.Id, ItemStatus.Completed);
        await LoadTasksAsync();
    }

    [RelayCommand]
    public async Task DeleteTaskAsync(TaskDisplay? taskDisplay)
    {
        if (taskDisplay?.TaskItem == null) return;
        await _taskService.DeleteAsync(taskDisplay.TaskItem);
        await LoadTasksAsync();
    }
}

/// <summary>
/// Helper para mostrar tareas con metadata de UI.
/// </summary>
public class TaskDisplay
{
    public TaskItem TaskItem { get; set; } = null!;
    public bool IsShortAnticipation { get; set; }
    public string? SubjectName { get; set; }
    public string SubjectColor { get; set; } = "#6B7280";
}