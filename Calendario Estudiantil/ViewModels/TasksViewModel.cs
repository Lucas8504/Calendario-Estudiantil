using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;

namespace Calendario_Estudiantil.ViewModels;

/// <summary>
/// ViewModel para gestión de tareas.
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private ObservableCollection<TaskDisplay> _tasks = new();

    [ObservableProperty]
    private string _filterMode = "Todos"; // Todos | Hoy | Pendientes

    [ObservableProperty]
    private bool _isLoading;

    public TasksViewModel(ITaskService taskService)
    {
        _taskService = taskService;
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
                IsShortAnticipation = _taskService.IsShortAnticipation(t.NotificationHours)
            }).ToList();

            Tasks = new ObservableCollection<TaskDisplay>(display);
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
}