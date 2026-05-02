using SQLite;
using Calendario_Estudiantil.Models;
using System.Diagnostics;

namespace Calendario_Estudiantil.Data;

/// <summary>
/// Servicio de base de datos SQLite usando sqlite-net-pcl.
/// Implementa IDatabaseService para inyección de dependencias.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly Lazy<SQLiteAsyncConnection> _database;

    /// <summary>
    /// Constructor: inicializa la conexión de forma lazy (se crea solo cuando se usa).
    /// </summary>
    public DatabaseService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "calendario.db3");

        _database = new Lazy<SQLiteAsyncConnection>(() =>
        {
            var database = new SQLiteAsyncConnection(dbPath);
            InitializeDatabase(database);
            return database;
        });
    }

    /// <summary>
    /// Inicializa las tablas de la base de datos.
    /// </summary>
    private void InitializeDatabase(SQLiteAsyncConnection database)
    {
        try
        {
            database.CreateTableAsync<Course>().Wait();
            database.CreateTableAsync<Subject>().Wait();
            database.CreateTableAsync<Schedule>().Wait();
            database.CreateTableAsync<TaskItem>().Wait();
            database.CreateTableAsync<Subtask>().Wait();
            database.CreateTableAsync<PreferenceRow>().Wait();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error inicializando BD: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la conexión a la base de datos (crea tablas si no existen).
    /// </summary>
    public SQLiteAsyncConnection Database => _database.Value;

    // ==================== Course ====================

    public async Task<List<Course>> GetAllCoursesAsync()
        => await Database.Table<Course>().ToListAsync();

    public async Task<Course> GetCourseByIdAsync(int id)
        => await Database.Table<Course>().Where(c => c.Id == id).FirstOrDefaultAsync()!;

    public Task<int> InsertCourseAsync(Course course)
        => Database.InsertAsync(course);

    public Task<int> UpdateCourseAsync(Course course)
        => Database.UpdateAsync(course);

    public Task<int> DeleteCourseAsync(Course course)
        => Database.DeleteAsync(course);

    // ==================== Subject ====================

    public async Task<List<Subject>> GetAllSubjectsAsync()
        => await Database.Table<Subject>().ToListAsync();

    public async Task<List<Subject>> GetSubjectsByCourseAsync(int courseId)
        => await Database.Table<Subject>().Where(s => s.CourseId == courseId).ToListAsync();

    public async Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return new List<Subject>();
        
        // Use SQL IN clause for better performance (avoid loading all subjects)
        var idString = string.Join(",", idList);
        return await Database.QueryAsync<Subject>($"SELECT * FROM Subject WHERE Id IN ({idString})");
    }

    public async Task<Subject> GetSubjectByIdAsync(int id)
        => await Database.Table<Subject>().Where(s => s.Id == id).FirstOrDefaultAsync()!;

    public Task<int> InsertSubjectAsync(Subject subject)
        => Database.InsertAsync(subject);

    public Task<int> UpdateSubjectAsync(Subject subject)
        => Database.UpdateAsync(subject);

    public Task<int> DeleteSubjectAsync(Subject subject)
        => Database.DeleteAsync(subject);

    public Task<int> DeleteSubjectsByCourseAsync(int courseId)
        => Database.Table<Subject>().Where(s => s.CourseId == courseId).DeleteAsync();

    // ==================== Schedule ====================

    public async Task<List<Schedule>> GetAllSchedulesAsync()
        => await Database.Table<Schedule>().ToListAsync();

    public async Task<List<Schedule>> GetSchedulesBySubjectAsync(int subjectId)
        => await Database.Table<Schedule>().Where(s => s.SubjectId == subjectId).ToListAsync();

    public async Task<List<Schedule>> GetSchedulesByDayAsync(DiaSemana day)
        => await Database.Table<Schedule>().Where(s => s.Day == day).ToListAsync();

    public Task<int> InsertScheduleAsync(Schedule schedule)
        => Database.InsertAsync(schedule);

    public Task<int> UpdateScheduleAsync(Schedule schedule)
        => Database.UpdateAsync(schedule);

    public Task<int> DeleteScheduleAsync(Schedule schedule)
        => Database.DeleteAsync(schedule);

    public Task<int> DeleteSchedulesBySubjectAsync(int subjectId)
        => Database.Table<Schedule>().Where(s => s.SubjectId == subjectId).DeleteAsync();

    // ==================== Batch Loading Methods (N+1 Prevention) ====================

    /// <summary>
    /// Loads all schedules for multiple subjects in a single query (prevents N+1).
    /// Returns a dictionary mapping SubjectId -> List of Schedules.
    /// </summary>
    public async Task<Dictionary<int, List<Schedule>>> GetSchedulesForSubjectsAsync(IEnumerable<int> subjectIds)
    {
        var idList = subjectIds.ToList();
        if (!idList.Any()) return new Dictionary<int, List<Schedule>>();
        
        var idString = string.Join(",", idList);
        var allSchedules = await Database.QueryAsync<Schedule>($"SELECT * FROM Schedule WHERE SubjectId IN ({idString})");
        
        var result = new Dictionary<int, List<Schedule>>();
        foreach (var schedule in allSchedules)
        {
            if (!result.ContainsKey(schedule.SubjectId))
                result[schedule.SubjectId] = new List<Schedule>();
            result[schedule.SubjectId].Add(schedule);
        }
        return result;
    }

    /// <summary>
    /// Loads all subtasks for multiple tasks in a single query (prevents N+1).
    /// Returns a dictionary mapping TaskItemId -> List of Subtasks.
    /// </summary>
    public async Task<Dictionary<int, List<Subtask>>> GetSubtasksForTasksAsync(IEnumerable<int> taskIds)
    {
        var idList = taskIds.ToList();
        if (!idList.Any()) return new Dictionary<int, List<Subtask>>();
        
        var idString = string.Join(",", idList);
        var allSubtasks = await Database.QueryAsync<Subtask>($"SELECT * FROM Subtask WHERE TaskItemId IN ({idString})");
        
        var result = new Dictionary<int, List<Subtask>>();
        foreach (var subtask in allSubtasks)
        {
            if (!result.ContainsKey(subtask.TaskItemId))
                result[subtask.TaskItemId] = new List<Subtask>();
            result[subtask.TaskItemId].Add(subtask);
        }
        return result;
    }

    // ==================== TaskItem ====================

    public async Task<List<TaskItem>> GetAllTasksAsync()
        => await Database.Table<TaskItem>().ToListAsync();

    public async Task<List<TaskItem>> GetTasksBySubjectAsync(int subjectId)
        => await Database.Table<TaskItem>().Where(t => t.SubjectId == subjectId).ToListAsync();

    public async Task<List<TaskItem>> GetTasksForTodayAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return await Database.Table<TaskItem>()
            .Where(t => t.DueDate >= today && t.DueDate < tomorrow)
            .ToListAsync();
    }

    public async Task<List<TaskItem>> GetPendingTasksAsync()
    {
        return await Database.Table<TaskItem>()
            .Where(t => t.Status == ItemStatus.Pending || t.Status == ItemStatus.InProgress)
            .ToListAsync();
    }

    public async Task<TaskItem> GetTaskByIdAsync(int id)
        => await Database.Table<TaskItem>().Where(t => t.Id == id).FirstOrDefaultAsync()!;

    public Task<int> InsertTaskAsync(TaskItem task)
        => Database.InsertAsync(task);

    public Task<int> UpdateTaskAsync(TaskItem task)
        => Database.UpdateAsync(task);

    public Task<int> DeleteTaskAsync(TaskItem task)
        => Database.DeleteAsync(task);

    public Task<int> DeleteTasksBySubjectAsync(int subjectId)
        => Database.Table<TaskItem>().Where(t => t.SubjectId == subjectId).DeleteAsync();

    // ==================== Subtask ====================

    public async Task<List<Subtask>> GetSubtasksByTaskAsync(int taskItemId)
        => await Database.Table<Subtask>().Where(s => s.TaskItemId == taskItemId).ToListAsync();

    public Task<int> InsertSubtaskAsync(Subtask subtask)
        => Database.InsertAsync(subtask);

    public Task<int> UpdateSubtaskAsync(Subtask subtask)
        => Database.UpdateAsync(subtask);

    public Task<int> DeleteSubtaskAsync(Subtask subtask)
        => Database.DeleteAsync(subtask);

    public Task<int> DeleteSubtasksByTaskAsync(int taskItemId)
        => Database.Table<Subtask>().Where(s => s.TaskItemId == taskItemId).DeleteAsync();

    // ==================== Preferencias (SQLite) ====================

    private int _activeCourseId = -2; // -2 = no leído aún

    public async Task<int> GetActiveCourseIdAsync()
    {
        if (_activeCourseId != -2) return _activeCourseId;

        var rows = await Database.Table<PreferenceRow>().ToListAsync();
        var pref = rows.FirstOrDefault(p => p.Key == "ActiveCourseId");
        _activeCourseId = pref != null && int.TryParse(pref.Value, out var id) ? id : -1;
        return _activeCourseId;
    }

    public async Task SetActiveCourseIdAsync(int courseId)
    {
        _activeCourseId = courseId;
        var rows = await Database.Table<PreferenceRow>().Where(p => p.Key == "ActiveCourseId").ToListAsync();

        if (rows.Any())
        {
            var row = rows.First();
            row.Value = courseId.ToString();
            await Database.UpdateAsync(row);
        }
        else
        {
            await Database.InsertAsync(new PreferenceRow { Key = "ActiveCourseId", Value = courseId.ToString() });
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
