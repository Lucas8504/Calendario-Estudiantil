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
    /// Constructor: crea la conexión lazy y dispara la creación de tablas async.
    /// sqlite-net CreateTableAsync es idempotente (IF NOT EXISTS).
    /// </summary>
    public DatabaseService()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "calendario.db3");

        _database = new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(dbPath));

        // Disparar creación de tablas en segundo plano (sin bloqueo)
        _ = InitializeTablesAsync();
    }

    /// <summary>
    /// Crea las tablas si no existen. Idempotente.
    /// </summary>
    private async Task InitializeTablesAsync()
    {
        try
        {
            var db = _database.Value;
            await db.CreateTableAsync<Subject>();
            await db.CreateTableAsync<Schedule>();
            await db.CreateTableAsync<TaskItem>();
            await db.CreateTableAsync<Subtask>();
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

    // ==================== Subject ====================

    public async Task<List<Subject>> GetAllSubjectsAsync()
        => await Database.Table<Subject>().ToListAsync();

    public async Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return new List<Subject>();
        
        return await Database.Table<Subject>()
            .Where(s => idList.Contains(s.Id))
            .ToListAsync();
    }

    public async Task<Subject> GetSubjectByIdAsync(int id)
        => await Database.Table<Subject>().Where(s => s.Id == id).FirstOrDefaultAsync()!;

    public Task<int> InsertSubjectAsync(Subject subject)
        => Database.InsertAsync(subject);

    public Task<int> UpdateSubjectAsync(Subject subject)
        => Database.UpdateAsync(subject);

    public Task<int> DeleteSubjectAsync(Subject subject)
        => Database.DeleteAsync(subject);

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
        
        var allSchedules = await Database.Table<Schedule>()
            .Where(s => idList.Contains(s.SubjectId))
            .ToListAsync();
        
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
        
        var allSubtasks = await Database.Table<Subtask>()
            .Where(s => idList.Contains(s.TaskItemId))
            .ToListAsync();
        
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
}
