using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de tareas.
/// </summary>
public class TaskService : ITaskService
{
    private readonly INotificationService _notificationService;
    private readonly IDatabaseService _databaseService;
    private List<TaskItem>? _tasksCache;

    public TaskService(INotificationService notificationService, IDatabaseService databaseService)
    {
        _notificationService = notificationService;
        _databaseService = databaseService;
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        if (_tasksCache != null) return _tasksCache;
        _tasksCache = await _databaseService.GetAllTasksAsync();
        return _tasksCache;
    }

    public Task<List<TaskItem>> GetBySubjectAsync(int subjectId)
        => _databaseService.GetTasksBySubjectAsync(subjectId);

    public Task<List<TaskItem>> GetForTodayAsync()
        => _databaseService.GetTasksForTodayAsync();

    public Task<List<TaskItem>> GetPendingAsync(int daysBack = 60, int daysForward = 60)
        => _databaseService.GetPendingTasksAsync(daysBack, daysForward);

    public Task<TaskItem> GetByIdAsync(int id)
        => _databaseService.GetTaskByIdAsync(id);

    public async Task<int> CreateAsync(TaskItem task)
    {
        var id = await _databaseService.InsertTaskAsync(task);
        if (task.Status == ItemStatus.Pending)
        {
            task.Id = id;
            await ScheduleNotificationAsync(task);
        }
        InvalidateCache();
        return id;
    }

    public async Task<int> UpdateAsync(TaskItem task)
    {
        var result = await _databaseService.UpdateTaskAsync(task);
        InvalidateCache();
        return result;
    }

    public async Task<int> UpdateStatusAsync(int taskId, ItemStatus status)
    {
        var task = await _databaseService.GetTaskByIdAsync(taskId);
        if (task == null) return -1;

        task.Status = status;
        await _databaseService.UpdateTaskAsync(task);

        if (status == ItemStatus.Completed)
            await CancelNotificationAsync(taskId);

        InvalidateCache();
        return taskId;
    }

    public async Task<int> DeleteAsync(TaskItem task)
    {
        await CancelNotificationAsync(task.Id);
        await _databaseService.DeleteSubtasksByTaskAsync(task.Id);
        var result = await _databaseService.DeleteTaskAsync(task);
        InvalidateCache();
        return result;
    }

    private void InvalidateCache()
    {
        _tasksCache = null;
    }

    public Task<List<Subtask>> GetSubtasksAsync(int taskItemId)
        => _databaseService.GetSubtasksByTaskAsync(taskItemId);

    public Task<int> AddSubtaskAsync(Subtask subtask)
        => _databaseService.InsertSubtaskAsync(subtask);

    public Task<int> UpdateSubtaskAsync(Subtask subtask)
        => _databaseService.UpdateSubtaskAsync(subtask);

    public Task<int> DeleteSubtaskAsync(Subtask subtask)
        => _databaseService.DeleteSubtaskAsync(subtask);

    /// <summary>
    /// Loads all subtasks for multiple tasks in a single query (prevents N+1).
    /// </summary>
    public Task<Dictionary<int, List<Subtask>>> GetSubtasksForTasksAsync(IEnumerable<int> taskIds)
        => _databaseService.GetSubtasksForTasksAsync(taskIds);

    public async Task ScheduleNotificationAsync(TaskItem task)
    {
        var notifyAt = task.DueDate.AddHours(-task.NotificationHours);
        if (notifyAt > DateTime.Now)
        {
            await _notificationService.ScheduleAsync(
                task.Id,
                task.Title,
                $"Vence: {task.DueDate:g}",
                notifyAt);
        }
    }

    public Task CancelNotificationAsync(int taskId)
        => _notificationService.CancelAsync(taskId);

    /// <summary>
    /// True si la anticipación es menor a 12 horas (mostrar warning rojo).
    /// </summary>
    public bool IsShortAnticipation(int hours) => hours < 12;
}