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

    // ==================== Course ====================
    Task<List<Course>> GetAllCoursesAsync();
    Task<Course> GetCourseByIdAsync(int id);
    Task<int> InsertCourseAsync(Course course);
    Task<int> UpdateCourseAsync(Course course);
    Task<int> DeleteCourseAsync(Course course);

    // ==================== Subject ====================
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<List<Subject>> GetSubjectsByCourseAsync(int courseId);
    Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids);
    Task<Subject> GetSubjectByIdAsync(int id);
    Task<int> InsertSubjectAsync(Subject subject);
    Task<int> UpdateSubjectAsync(Subject subject);
    Task<int> DeleteSubjectAsync(Subject subject);
    Task<int> DeleteSubjectsByCourseAsync(int courseId);

    // ==================== Schedule ====================
    Task<List<Schedule>> GetAllSchedulesAsync();
    Task<List<Schedule>> GetSchedulesBySubjectAsync(int subjectId);
    Task<List<Schedule>> GetSchedulesByDayAsync(DiaSemana day);
    Task<int> InsertScheduleAsync(Schedule schedule);
    Task<int> UpdateScheduleAsync(Schedule schedule);
    Task<int> DeleteScheduleAsync(Schedule schedule);
    Task<int> DeleteSchedulesBySubjectAsync(int subjectId);

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

    // ==================== Preferencias ====================
    Task<int> GetActiveCourseIdAsync();
    Task SetActiveCourseIdAsync(int courseId);
}
