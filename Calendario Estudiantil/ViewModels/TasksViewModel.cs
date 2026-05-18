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

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Todas las tareas sin filtrar (para búsqueda)
    private List<TaskDisplay> _allTasks = new();

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

            _allTasks = display;
            ApplySearchFilter();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Aplica el filtro de búsqueda sobre las tareas.
    /// </summary>
    private void ApplySearchFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allTasks
            : _allTasks.Where(t => 
                t.TaskItem.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                t.TaskItem.Description?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true)
              .ToList();

        Tasks = new ObservableCollection<TaskDisplay>(filtered);
    }

    /// <summary>
    /// Busca tareas por título o descripción.
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