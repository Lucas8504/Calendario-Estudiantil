using Calendario_Estudiantil.Data;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services;
using Calendario_Estudiantil.Services.Voice;

namespace Calendario_Estudiantil.Voice.Tests.Fakes;

/// <summary>
/// Almacén en memoria compartido por los fakes de servicios.
/// </summary>
public sealed class InMemoryStore
{
    private int _nextSubjectId = 1;
    private int _nextTaskId = 1;
    private int _nextScheduleId = 1;

    public List<Subject> Subjects { get; } = new();
    public List<TaskItem> Tasks { get; } = new();
    public List<Schedule> Schedules { get; } = new();

    public Subject AddSubject(string name, string color = "#512BD4")
    {
        var subject = new Subject { Id = _nextSubjectId++, Name = name, ColorHex = color };
        Subjects.Add(subject);
        return subject;
    }

    public TaskItem AddTask(int subjectId, string title, DateTime dueDate, ItemStatus status = ItemStatus.Pending)
    {
        var task = new TaskItem
        {
            Id = _nextTaskId++,
            SubjectId = subjectId,
            Title = title,
            DueDate = dueDate,
            Status = status
        };
        Tasks.Add(task);
        return task;
    }

    public Schedule AddSchedule(int subjectId, DiaSemana day, TimeSpan start, TimeSpan end)
    {
        var schedule = new Schedule
        {
            Id = _nextScheduleId++,
            SubjectId = subjectId,
            Day = day,
            StartTime = start,
            EndTime = end
        };
        Schedules.Add(schedule);
        return schedule;
    }

    public int NextTaskId() => _nextTaskId++;
    public int NextScheduleId() => _nextScheduleId++;
}

public sealed class FakeSubjectService : ISubjectService
{
    private readonly InMemoryStore _store;

    public FakeSubjectService(InMemoryStore store) => _store = store;

    public Task<List<Subject>> GetAllAsync() => Task.FromResult(_store.Subjects.ToList());

    public Task<Subject> GetByIdAsync(int id) =>
        Task.FromResult(_store.Subjects.FirstOrDefault(s => s.Id == id)!);

    public Task<int> CreateAsync(Subject subject)
    {
        subject.Id = _store.Subjects.Count + 1;
        _store.Subjects.Add(subject);
        return Task.FromResult(subject.Id);
    }

    public Task<int> UpdateAsync(Subject subject) => Task.FromResult(1);

    public Task<int> DeleteAsync(Subject subject)
    {
        _store.Subjects.RemoveAll(s => s.Id == subject.Id);
        return Task.FromResult(1);
    }

    public Task<List<Schedule>> GetSchedulesAsync(int subjectId) =>
        Task.FromResult(_store.Schedules.Where(s => s.SubjectId == subjectId).ToList());

    public Task<int> AddScheduleAsync(Schedule schedule)
    {
        schedule.Id = _store.NextScheduleId();
        _store.Schedules.Add(schedule);
        return Task.FromResult(schedule.Id);
    }

    public Task<int> UpdateScheduleAsync(Schedule schedule) => Task.FromResult(1);

    public Task<int> DeleteScheduleAsync(Schedule schedule)
    {
        _store.Schedules.RemoveAll(s => s.Id == schedule.Id);
        return Task.FromResult(1);
    }
}

public sealed class FakeTaskService : ITaskService
{
    private readonly InMemoryStore _store;

    public FakeTaskService(InMemoryStore store) => _store = store;

    public Task<List<TaskItem>> GetAllAsync() => Task.FromResult(_store.Tasks.ToList());

    public Task<List<TaskItem>> GetBySubjectAsync(int subjectId) =>
        Task.FromResult(_store.Tasks.Where(t => t.SubjectId == subjectId).ToList());

    public Task<List<TaskItem>> GetForTodayAsync() =>
        Task.FromResult(_store.Tasks.Where(t => t.DueDate.Date == DateTime.Today).ToList());

    public Task<List<TaskItem>> GetPendingAsync(int daysBack = 60, int daysForward = 60) =>
        Task.FromResult(_store.Tasks.Where(t => t.Status == ItemStatus.Pending).ToList());

    public Task<TaskItem> GetByIdAsync(int id) =>
        Task.FromResult(_store.Tasks.FirstOrDefault(t => t.Id == id)!);

    public Task<int> CreateAsync(TaskItem task)
    {
        task.Id = _store.NextTaskId();
        _store.Tasks.Add(task);
        return Task.FromResult(task.Id);
    }

    public Task<int> UpdateAsync(TaskItem task) => Task.FromResult(1);

    public Task<int> UpdateStatusAsync(int taskId, ItemStatus status)
    {
        var task = _store.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is not null)
            task.Status = status;
        return Task.FromResult(1);
    }

    public Task<int> DeleteAsync(TaskItem task)
    {
        _store.Tasks.RemoveAll(t => t.Id == task.Id);
        return Task.FromResult(1);
    }

    public Task<List<Subtask>> GetSubtasksAsync(int taskItemId) =>
        Task.FromResult(new List<Subtask>());

    public Task<int> AddSubtaskAsync(Subtask subtask) => Task.FromResult(1);

    public Task<int> UpdateSubtaskAsync(Subtask subtask) => Task.FromResult(1);

    public Task<int> DeleteSubtaskAsync(Subtask subtask) => Task.FromResult(1);

    public Task ScheduleNotificationAsync(TaskItem task) => Task.CompletedTask;

    public Task CancelNotificationAsync(int taskId) => Task.CompletedTask;

    public bool IsShortAnticipation(int hours) => hours < 12;
}

public sealed class FakeDatabaseService : IDatabaseService
{
    private readonly InMemoryStore _store;

    public FakeDatabaseService(InMemoryStore store) => _store = store;

    public SQLite.SQLiteAsyncConnection Database =>
        throw new NotSupportedException("La base real no se usa en tests.");

    public Task<List<Schedule>> GetAllSchedulesAsync() => Task.FromResult(_store.Schedules.ToList());

    public Task<int> DeleteSchedulesBySubjectAsync(int subjectId)
    {
        int removed = _store.Schedules.RemoveAll(s => s.SubjectId == subjectId);
        return Task.FromResult(removed);
    }

    public Task<int> DeleteTasksBySubjectAsync(int subjectId)
    {
        int removed = _store.Tasks.RemoveAll(t => t.SubjectId == subjectId);
        return Task.FromResult(removed);
    }

    public Task<List<Subject>> GetAllSubjectsAsync() => throw new NotSupportedException();
    public Task<List<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids) => throw new NotSupportedException();
    public Task<Subject> GetSubjectByIdAsync(int id) => throw new NotSupportedException();
    public Task<int> InsertSubjectAsync(Subject subject) => throw new NotSupportedException();
    public Task<int> UpdateSubjectAsync(Subject subject) => throw new NotSupportedException();
    public Task<int> DeleteSubjectAsync(Subject subject) => throw new NotSupportedException();
    public Task<List<Schedule>> GetSchedulesBySubjectAsync(int subjectId) => throw new NotSupportedException();
    public Task<List<Schedule>> GetSchedulesByDayAsync(DiaSemana day) => throw new NotSupportedException();
    public Task<int> InsertScheduleAsync(Schedule schedule) => throw new NotSupportedException();
    public Task<int> UpdateScheduleAsync(Schedule schedule) => throw new NotSupportedException();
    public Task<int> DeleteScheduleAsync(Schedule schedule) => throw new NotSupportedException();
    public Task<Dictionary<int, List<Schedule>>> GetSchedulesForSubjectsAsync(IEnumerable<int> subjectIds) => throw new NotSupportedException();
    public Task<List<TaskItem>> GetAllTasksAsync() => throw new NotSupportedException();
    public Task<List<TaskItem>> GetTasksBySubjectAsync(int subjectId) => throw new NotSupportedException();
    public Task<List<TaskItem>> GetTasksForTodayAsync() => throw new NotSupportedException();
    public Task<List<TaskItem>> GetPendingTasksAsync(int daysBack = 60, int daysForward = 60) => throw new NotSupportedException();
    public Task<TaskItem> GetTaskByIdAsync(int id) => throw new NotSupportedException();
    public Task<int> InsertTaskAsync(TaskItem task) => throw new NotSupportedException();
    public Task<int> UpdateTaskAsync(TaskItem task) => throw new NotSupportedException();
    public Task<int> DeleteTaskAsync(TaskItem task) => throw new NotSupportedException();
    public Task<List<Subtask>> GetSubtasksByTaskAsync(int taskItemId) => throw new NotSupportedException();
    public Task<int> InsertSubtaskAsync(Subtask subtask) => throw new NotSupportedException();
    public Task<int> UpdateSubtaskAsync(Subtask subtask) => throw new NotSupportedException();
    public Task<int> DeleteSubtaskAsync(Subtask subtask) => throw new NotSupportedException();
    public Task<int> DeleteSubtasksByTaskAsync(int taskItemId) => throw new NotSupportedException();
    public Task<Dictionary<int, List<Subtask>>> GetSubtasksForTasksAsync(IEnumerable<int> taskIds) => throw new NotSupportedException();
}

public sealed class FakeGeminiClient : IGeminiClient
{
    public VoiceCommandResult Result { get; set; } = new(new List<ToolCall>(), null);

    public int CallCount { get; private set; }

    public List<FunctionDeclaration>? LastTools { get; private set; }

    public DateTime LastToday { get; private set; }

    public Task<VoiceCommandResult> GetVoiceCommandsAsync(
        Stream audioStream,
        List<FunctionDeclaration> tools,
        DateTime today,
        CancellationToken ct = default)
    {
        CallCount++;
        LastTools = tools;
        LastToday = today;
        return Task.FromResult(Result);
    }
}
