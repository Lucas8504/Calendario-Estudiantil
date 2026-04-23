using SQLite;
using Calendario_Estudiantil.Models;

namespace Calendario_Estudiantil.Data;

/// <summary>
/// Servicio de base de datos SQLite usando sqlite-net-pcl.
/// </summary>
public static class DatabaseService
{
    private static SQLiteAsyncConnection? _database;

    /// <summary>
    /// Obtiene la conexión a la base de datos (crea tablas si no existen).
    /// </summary>
    public static SQLiteAsyncConnection Database
    {
        get
        {
            if (_database == null)
            {
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "calendario.db3");

                var database = new SQLiteAsyncConnection(dbPath);
                database.CreateTableAsync<Course>().Wait();
                database.CreateTableAsync<Subject>().Wait();
                database.CreateTableAsync<Schedule>().Wait();
                database.CreateTableAsync<TaskItem>().Wait();
                database.CreateTableAsync<Subtask>().Wait();
                database.CreateTableAsync<PreferenceRow>().Wait();

                _database = database;
            }

            return _database;
        }
    }

    // ==================== Course ====================

    public static Task<List<Course>> GetAllCoursesAsync()
        => Database.Table<Course>().ToListAsync();

    public static Task<Course?> GetCourseByIdAsync(int id)
        => Database.Table<Course>().Where(c => c.Id == id).FirstOrDefaultAsync();

    public static Task<int> InsertCourseAsync(Course course)
        => Database.InsertAsync(course);

    public static Task<int> UpdateCourseAsync(Course course)
        => Database.UpdateAsync(course);

    public static Task<int> DeleteCourseAsync(Course course)
        => Database.DeleteAsync(course);

    // ==================== Subject ====================

    public static Task<List<Subject>> GetAllSubjectsAsync()
        => Database.Table<Subject>().ToListAsync();

    public static Task<List<Subject>> GetSubjectsByCourseAsync(int courseId)
        => Database.Table<Subject>().Where(s => s.CourseId == courseId).ToListAsync();

    public static Task<Subject?> GetSubjectByIdAsync(int id)
        => Database.Table<Subject>().Where(s => s.Id == id).FirstOrDefaultAsync();

    public static Task<int> InsertSubjectAsync(Subject subject)
        => Database.InsertAsync(subject);

    public static Task<int> UpdateSubjectAsync(Subject subject)
        => Database.UpdateAsync(subject);

    public static Task<int> DeleteSubjectAsync(Subject subject)
        => Database.DeleteAsync(subject);

    public static Task<int> DeleteSubjectsByCourseAsync(int courseId)
        => Database.Table<Subject>().Where(s => s.CourseId == courseId).DeleteAsync();

    // ==================== Schedule ====================

    public static Task<List<Schedule>> GetSchedulesBySubjectAsync(int subjectId)
        => Database.Table<Schedule>().Where(s => s.SubjectId == subjectId).ToListAsync();

    public static Task<List<Schedule>> GetSchedulesByDayAsync(DiaSemana day)
        => Database.Table<Schedule>().Where(s => s.Day == day).ToListAsync();

    public static Task<int> InsertScheduleAsync(Schedule schedule)
        => Database.InsertAsync(schedule);

    public static Task<int> UpdateScheduleAsync(Schedule schedule)
        => Database.UpdateAsync(schedule);

    public static Task<int> DeleteScheduleAsync(Schedule schedule)
        => Database.DeleteAsync(schedule);

    public static Task<int> DeleteSchedulesBySubjectAsync(int subjectId)
        => Database.Table<Schedule>().Where(s => s.SubjectId == subjectId).DeleteAsync();

    // ==================== TaskItem ====================

    public static Task<List<TaskItem>> GetAllTasksAsync()
        => Database.Table<TaskItem>().ToListAsync();

    public static Task<List<TaskItem>> GetTasksBySubjectAsync(int subjectId)
        => Database.Table<TaskItem>().Where(t => t.SubjectId == subjectId).ToListAsync();

    public static Task<List<TaskItem>> GetTasksForTodayAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return Database.Table<TaskItem>()
            .Where(t => t.DueDate >= today && t.DueDate < tomorrow)
            .ToListAsync();
    }

    public static Task<List<TaskItem>> GetPendingTasksAsync()
    {
        return Database.Table<TaskItem>()
            .Where(t => t.Status == ItemStatus.Pending || t.Status == ItemStatus.InProgress)
            .ToListAsync();
    }

    public static Task<TaskItem?> GetTaskByIdAsync(int id)
        => Database.Table<TaskItem>().Where(t => t.Id == id).FirstOrDefaultAsync();

    public static Task<int> InsertTaskAsync(TaskItem task)
        => Database.InsertAsync(task);

    public static Task<int> UpdateTaskAsync(TaskItem task)
        => Database.UpdateAsync(task);

    public static Task<int> DeleteTaskAsync(TaskItem task)
        => Database.DeleteAsync(task);

    public static Task<int> DeleteTasksBySubjectAsync(int subjectId)
        => Database.Table<TaskItem>().Where(t => t.SubjectId == subjectId).DeleteAsync();

    // ==================== Subtask ====================

    public static Task<List<Subtask>> GetSubtasksByTaskAsync(int taskItemId)
        => Database.Table<Subtask>().Where(s => s.TaskItemId == taskItemId).ToListAsync();

    public static Task<int> InsertSubtaskAsync(Subtask subtask)
        => Database.InsertAsync(subtask);

    public static Task<int> UpdateSubtaskAsync(Subtask subtask)
        => Database.UpdateAsync(subtask);

    public static Task<int> DeleteSubtaskAsync(Subtask subtask)
        => Database.DeleteAsync(subtask);

    public static Task<int> DeleteSubtasksByTaskAsync(int taskItemId)
        => Database.Table<Subtask>().Where(s => s.TaskItemId == taskItemId).DeleteAsync();

    // ==================== Preferencias (SQLite) ====================

    private static int _activeCourseId = -2; // -2 = no leído aún

    public static async Task<int> GetActiveCourseIdAsync()
    {
        if (_activeCourseId != -2) return _activeCourseId;

        var db = Database;
        var rows = await db.Table<PreferenceRow>().ToListAsync();
        var pref = rows.FirstOrDefault(p => p.Key == "ActiveCourseId");
        _activeCourseId = pref != null && int.TryParse(pref.Value, out var id) ? id : -1;
        return _activeCourseId;
    }

    public static async Task SetActiveCourseIdAsync(int courseId)
    {
        _activeCourseId = courseId;
        var db = Database;
        var rows = await db.Table<PreferenceRow>().Where(p => p.Key == "ActiveCourseId").ToListAsync();

        if (rows.Any())
        {
            var row = rows.First();
            row.Value = courseId.ToString();
            await db.UpdateAsync(row);
        }
        else
        {
            await db.InsertAsync(new PreferenceRow { Key = "ActiveCourseId", Value = courseId.ToString() });
        }
    }

    /// <summary>
    /// Fila genérica para preferencias (clave-valor simple).
    /// </summary>
    private class PreferenceRow
    {
        [PrimaryKey]
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}