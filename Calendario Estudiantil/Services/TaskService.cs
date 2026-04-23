using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Implementación del servicio de tareas.
/// </summary>
public class TaskService : ITaskService
{
    private readonly INotificationService _notificationService;

    public TaskService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task<List<TaskItem>> GetAllAsync()
        => DatabaseService.GetAllTasksAsync();

    public Task<List<TaskItem>> GetBySubjectAsync(int subjectId)
        => DatabaseService.GetTasksBySubjectAsync(subjectId);

    public Task<List<TaskItem>> GetForTodayAsync()
        => DatabaseService.GetTasksForTodayAsync();

    public Task<List<TaskItem>> GetPendingAsync()
        => DatabaseService.GetPendingTasksAsync();

    public Task<TaskItem?> GetByIdAsync(int id)
        => DatabaseService.GetTaskByIdAsync(id);

    public async Task<int> CreateAsync(TaskItem task)
    {
        var id = await DatabaseService.InsertTaskAsync(task);
        if (task.Status == ItemStatus.Pending)
        {
            task.Id = id;
            await ScheduleNotificationAsync(task);
        }
        return id;
    }

    public Task<int> UpdateAsync(TaskItem task)
        => DatabaseService.UpdateTaskAsync(task);

    public async Task<int> UpdateStatusAsync(int taskId, ItemStatus status)
    {
        var task = await DatabaseService.GetTaskByIdAsync(taskId);
        if (task == null) return -1;

        task.Status = status;
        await DatabaseService.UpdateTaskAsync(task);

        if (status == ItemStatus.Completed)
            await CancelNotificationAsync(taskId);

        return taskId;
    }

    public async Task<int> DeleteAsync(TaskItem task)
    {
        await CancelNotificationAsync(task.Id);
        await DatabaseService.DeleteSubtasksByTaskAsync(task.Id);
        return await DatabaseService.DeleteTaskAsync(task);
    }

    public Task<List<Subtask>> GetSubtasksAsync(int taskItemId)
        => DatabaseService.GetSubtasksByTaskAsync(taskItemId);

    public Task<int> AddSubtaskAsync(Subtask subtask)
        => DatabaseService.InsertSubtaskAsync(subtask);

    public Task<int> UpdateSubtaskAsync(Subtask subtask)
        => DatabaseService.UpdateSubtaskAsync(subtask);

    public Task<int> DeleteSubtaskAsync(Subtask subtask)
        => DatabaseService.DeleteSubtaskAsync(subtask);

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