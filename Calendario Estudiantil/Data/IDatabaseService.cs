using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Data;

/// <summary>
/// Interfaz para el servicio de base de datos SQLite.
/// Permite inyección de dependencias y testing.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Obtiene la conexión a la base de datos.
    /// </summary>
    SQLiteAsyncConnection Database { get; }

    // ==================== Subject ====================
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids);
    Task<Subject> GetSubjectByIdAsync(int id);
    Task<int> InsertSubjectAsync(Subject subject);
    Task<int> UpdateSubjectAsync(Subject subject);
    Task<int> DeleteSubjectAsync(Subject subject);

    // ==================== Schedule ====================
    Task<List<Schedule>> GetAllSchedulesAsync();
    Task<List<Schedule>> GetSchedulesBySubjectAsync(int subjectId);
    Task<List<Schedule>> GetSchedulesByDayAsync(DiaSemana day);
    Task<int> InsertScheduleAsync(Schedule schedule);
    Task<int> UpdateScheduleAsync(Schedule schedule);
    Task<int> DeleteScheduleAsync(Schedule schedule);
    Task<int> DeleteSchedulesBySubjectAsync(int subjectId);
    Task<Dictionary<int, List<Schedule>>> GetSchedulesForSubjectsAsync(IEnumerable<int> subjectIds);

    // ==================== TaskItem ====================
    Task<List<TaskItem>> GetAllTasksAsync();
    Task<List<TaskItem>> GetTasksBySubjectAsync(int subjectId);
    Task<List<TaskItem>> GetTasksForTodayAsync();
    Task<List<TaskItem>> GetPendingTasksAsync();
    Task<TaskItem> GetTaskByIdAsync(int id);
    Task<int> InsertTaskAsync(TaskItem task);
    Task<int> UpdateTaskAsync(TaskItem task);
    Task<int> DeleteTaskAsync(TaskItem task);
    Task<int> DeleteTasksBySubjectAsync(int subjectId);

    // ==================== Subtask ====================
    Task<List<Subtask>> GetSubtasksByTaskAsync(int taskItemId);
    Task<int> InsertSubtaskAsync(Subtask subtask);
    Task<int> UpdateSubtaskAsync(Subtask subtask);
    Task<int> DeleteSubtaskAsync(Subtask subtask);
    Task<int> DeleteSubtasksByTaskAsync(int taskItemId);
    Task<Dictionary<int, List<Subtask>>> GetSubtasksForTasksAsync(IEnumerable<int> taskIds);

}
