using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Services;

/// <summary>
/// Servicio de tareas.
/// </summary>
public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync();
    Task<List<TaskItem>> GetBySubjectAsync(int subjectId);
    Task<List<TaskItem>> GetForTodayAsync();
    Task<List<TaskItem>> GetPendingAsync();
    Task<TaskItem> GetByIdAsync(int id);
    Task<int> CreateAsync(TaskItem task);
    Task<int> UpdateAsync(TaskItem task);
    Task<int> UpdateStatusAsync(int taskId, ItemStatus status);
    Task<int> DeleteAsync(TaskItem task);
    Task<List<Subtask>> GetSubtasksAsync(int taskItemId);
    Task<int> AddSubtaskAsync(Subtask subtask);
    Task<int> UpdateSubtaskAsync(Subtask subtask);
    Task<int> DeleteSubtaskAsync(Subtask subtask);
    Task ScheduleNotificationAsync(TaskItem task);
    Task CancelNotificationAsync(int taskId);
    bool IsShortAnticipation(int hours);
}