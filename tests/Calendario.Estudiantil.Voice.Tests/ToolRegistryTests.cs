using Xunit;
using System.Text.Json;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services.Voice;
using Calendario_Estudiantil.Voice.Tests.Fakes;

namespace Calendario_Estudiantil.Voice.Tests;

public class ToolRegistryTests
{
    private static readonly string[] ExpectedTools =
    {
        "find_subject",
        "add_task",
        "update_task",
        "delete_task",
        "add_subject",
        "update_subject",
        "delete_subject",
        "add_schedule",
        "update_schedule",
        "delete_schedule"
    };

    private static (ToolRegistry Registry, InMemoryStore Store) Create()
    {
        var store = new InMemoryStore();
        var registry = new ToolRegistry(
            new FakeTaskService(store),
            new FakeSubjectService(store),
            new FakeDatabaseService(store));
        return (registry, store);
    }

    [Fact]
    public void GetFunctionDeclarations_ReturnsAllTenTools()
    {
        var (registry, _) = Create();

        var names = registry.GetFunctionDeclarations().Select(d => d.Name).OrderBy(n => n).ToArray();

        Assert.Equal(ExpectedTools.OrderBy(n => n).ToArray(), names);
    }

    [Fact]
    public void AllFunctionDeclarations_HaveObjectSchemaWithProperties()
    {
        var (registry, _) = Create();

        foreach (var declaration in registry.GetFunctionDeclarations())
        {
            using var document = JsonDocument.Parse(declaration.ParametersJson);
            var root = document.RootElement;

            Assert.Equal("object", root.GetProperty("type").GetString());
            Assert.True(root.TryGetProperty("properties", out var properties));
            Assert.Equal(JsonValueKind.Object, properties.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(declaration.Description));
        }
    }

    [Fact]
    public async Task FindSubject_MatchesIgnoringAccents()
    {
        var (registry, store) = Create();
        store.AddSubject("Cálculo I");

        var result = await registry.ExecuteAsync("find_subject", """{"name":"calculo"}""");

        Assert.True(result.Success);
        Assert.Contains("Cálculo I", result.Message);
    }

    [Fact]
    public async Task FindSubject_QueryWithSpaces_MatchesSubjectWithHyphen()
    {
        var (registry, store) = Create();
        store.AddSubject("Físico-Química");

        var result = await registry.ExecuteAsync("find_subject", """{"name":"Físico Química"}""");

        Assert.True(result.Success);
        Assert.Contains("Físico-Química", result.Message);
    }

    [Fact]
    public async Task FindSubject_QueryWithHyphen_MatchesSubjectWithSpaces()
    {
        var (registry, store) = Create();
        store.AddSubject("Físico Química");

        var result = await registry.ExecuteAsync("find_subject", """{"name":"Físico-Química"}""");

        Assert.True(result.Success);
        Assert.Contains("Físico Química", result.Message);
    }

    [Fact]
    public async Task AddTask_SubjectNameWithHyphen_ResolvesSubjectStoredWithSpaces()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Físico Química");

        var result = await registry.ExecuteAsync("add_task",
            """{"title":"Práctica 3","subject_name":"Físico-Química","due_date":"2026-12-01T18:00:00"}""");

        Assert.True(result.Success);
        Assert.Equal(subject.Id, Assert.Single(store.Tasks).SubjectId);
    }

    [Fact]
    public async Task FindSubject_WithoutMatches_ReturnsFailure()
    {
        var (registry, _) = Create();

        var result = await registry.ExecuteAsync("find_subject", """{"name":"quimica"}""");

        Assert.False(result.Success);
        Assert.Contains("No encontré", result.Message);
    }

    [Fact]
    public async Task AddTask_CreatesTaskForResolvedSubject()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Física");

        var result = await registry.ExecuteAsync("add_task",
            """{"title":"TP de óptica","subject_name":"fisica","due_date":"2026-12-01T18:00:00"}""");

        Assert.True(result.Success);
        var task = Assert.Single(store.Tasks);
        Assert.Equal(subject.Id, task.SubjectId);
        Assert.Equal("TP de óptica", task.Title);
        Assert.Equal(new DateTime(2026, 12, 1, 18, 0, 0), task.DueDate);
        Assert.Equal(ItemStatus.Pending, task.Status);
        Assert.Equal(24, task.NotificationHours);
    }

    [Fact]
    public async Task AddTask_DateOnly_UsesEndOfDay()
    {
        var (registry, store) = Create();
        store.AddSubject("Historia");

        await registry.ExecuteAsync("add_task",
            """{"title":"Ensayo","subject_name":"historia","due_date":"2026-11-13"}""");

        var task = Assert.Single(store.Tasks);
        Assert.Equal(new DateTime(2026, 11, 13, 23, 59, 0), task.DueDate);
    }

    [Fact]
    public async Task AddTask_WithUnknownSubject_Fails()
    {
        var (registry, store) = Create();

        var result = await registry.ExecuteAsync("add_task",
            """{"title":"Algo","subject_name":"inexistente","due_date":"2026-12-01T10:00:00"}""");

        Assert.False(result.Success);
        Assert.Contains("materia", result.Message);
        Assert.Empty(store.Tasks);
    }

    [Fact]
    public async Task AddTask_WithInvalidDate_Fails()
    {
        var (registry, store) = Create();
        store.AddSubject("Historia");

        var result = await registry.ExecuteAsync("add_task",
            """{"title":"Algo","subject_name":"historia","due_date":"el martes que viene"}""");

        Assert.False(result.Success);
        Assert.Contains("fecha", result.Message);
        Assert.Empty(store.Tasks);
    }

    [Fact]
    public async Task UpdateTask_WithoutConfirmation_DoesNotMutate()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Matemática");
        var task = store.AddTask(subject.Id, "Original", new DateTime(2026, 11, 20, 10, 0, 0));

        var result = await registry.ExecuteAsync("update_task",
            $$"""{"task_id":{{task.Id}},"title":"Renombrada"}""");

        Assert.True(result.RequiresConfirmation);
        Assert.False(string.IsNullOrWhiteSpace(result.ConfirmationPrompt));
        Assert.Contains("Original", result.ConfirmationPrompt);
        Assert.Equal("Original", store.Tasks[0].Title);
    }

    [Fact]
    public async Task UpdateTask_Confirmed_AppliesAllChanges()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Matemática");
        var task = store.AddTask(subject.Id, "Original", new DateTime(2026, 11, 20, 10, 0, 0));

        string args =
            $$"""{"task_id":{{task.Id}},"title":"TP final","due_date":"2026-12-01T18:00:00","status":"completed","notification_hours":6}""";

        await registry.ExecuteAsync("update_task", args);
        var result = await registry.ExecuteConfirmedAsync("update_task", args);

        Assert.True(result.Success);
        var updated = store.Tasks[0];
        Assert.Equal("TP final", updated.Title);
        Assert.Equal(new DateTime(2026, 12, 1, 18, 0, 0), updated.DueDate);
        Assert.Equal(ItemStatus.Completed, updated.Status);
        Assert.Equal(6, updated.NotificationHours);
    }

    [Fact]
    public async Task UpdateTask_WithoutChanges_Fails()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Matemática");
        var task = store.AddTask(subject.Id, "Original", new DateTime(2026, 11, 20, 10, 0, 0));

        var result = await registry.ExecuteAsync("update_task", $$"""{"task_id":{{task.Id}}}""");

        Assert.False(result.Success);
        Assert.Contains("qué cambiar", result.Message);
    }

    [Fact]
    public async Task DeleteTask_RequiresConfirmation_ThenDeletes()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Lengua");
        var task = store.AddTask(subject.Id, "Leer capítulo 3", new DateTime(2026, 11, 15, 9, 0, 0));
        string args = $$"""{"task_id":{{task.Id}}}""";

        var pending = await registry.ExecuteAsync("delete_task", args);
        Assert.True(pending.RequiresConfirmation);
        Assert.Single(store.Tasks);

        var confirmed = await registry.ExecuteConfirmedAsync("delete_task", args);
        Assert.True(confirmed.Success);
        Assert.Empty(store.Tasks);
    }

    [Fact]
    public async Task AddSchedule_ParsesSpanishDayAndTimes()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Química");

        var result = await registry.ExecuteAsync("add_schedule",
            """{"subject_name":"quimica","day":"miércoles","start_time":"18:00","end_time":"20:30"}""");

        Assert.True(result.Success);
        var schedule = Assert.Single(store.Schedules);
        Assert.Equal(subject.Id, schedule.SubjectId);
        Assert.Equal(DiaSemana.Miércoles, schedule.Day);
        Assert.Equal(new TimeSpan(18, 0, 0), schedule.StartTime);
        Assert.Equal(new TimeSpan(20, 30, 0), schedule.EndTime);
    }

    [Fact]
    public async Task AddSchedule_WithEndBeforeStart_Fails()
    {
        var (registry, store) = Create();
        store.AddSubject("Química");

        var result = await registry.ExecuteAsync("add_schedule",
            """{"subject_name":"quimica","day":3,"start_time":"20:00","end_time":"18:00"}""");

        Assert.False(result.Success);
        Assert.Contains("posterior", result.Message);
        Assert.Empty(store.Schedules);
    }

    [Fact]
    public async Task UpdateSchedule_RequiresConfirmation_ThenApplies()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Química");
        var schedule = store.AddSchedule(subject.Id, DiaSemana.Lunes,
            new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0));
        string args = $$"""{"schedule_id":{{schedule.Id}},"day":5,"start_time":"08:00","end_time":"10:00"}""";

        var pending = await registry.ExecuteAsync("update_schedule", args);
        Assert.True(pending.RequiresConfirmation);
        Assert.Equal(DiaSemana.Lunes, store.Schedules[0].Day);

        var confirmed = await registry.ExecuteConfirmedAsync("update_schedule", args);
        Assert.True(confirmed.Success);
        Assert.Equal(DiaSemana.Viernes, store.Schedules[0].Day);
        Assert.Equal(new TimeSpan(8, 0, 0), store.Schedules[0].StartTime);
    }

    [Fact]
    public async Task DeleteSubject_PromptShowsCascadeCounts()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Biología");
        store.AddTask(subject.Id, "Tarea 1", new DateTime(2026, 11, 20, 10, 0, 0));
        store.AddTask(subject.Id, "Tarea 2", new DateTime(2026, 11, 21, 10, 0, 0));
        store.AddSchedule(subject.Id, DiaSemana.Martes, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));

        var result = await registry.ExecuteAsync("delete_subject", $$"""{"subject_id":{{subject.Id}}}""");

        Assert.True(result.RequiresConfirmation);
        Assert.Contains("2 tarea(s)", result.ConfirmationPrompt);
        Assert.Contains("1 horario(s)", result.ConfirmationPrompt);
    }

    [Fact]
    public async Task DeleteSubject_Confirmed_DeletesSubjectTasksAndSchedules()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Biología");
        store.AddTask(subject.Id, "Tarea 1", new DateTime(2026, 11, 20, 10, 0, 0));
        store.AddSchedule(subject.Id, DiaSemana.Martes, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));
        string args = $$"""{"subject_id":{{subject.Id}}}""";

        await registry.ExecuteAsync("delete_subject", args);
        var result = await registry.ExecuteConfirmedAsync("delete_subject", args);

        Assert.True(result.Success);
        Assert.Empty(store.Subjects);
        Assert.Empty(store.Tasks);
        Assert.Empty(store.Schedules);
    }

    [Fact]
    public async Task AddSubject_WithExistingName_Fails()
    {
        var (registry, store) = Create();
        store.AddSubject("Álgebra");

        var result = await registry.ExecuteAsync("add_subject", """{"name":"algebra"}""");

        Assert.False(result.Success);
        Assert.Contains("ya existe", result.Message);
        Assert.Single(store.Subjects);
    }

    [Fact]
    public async Task AddSubject_CreatesWithDefaultColor()
    {
        var (registry, store) = Create();

        var result = await registry.ExecuteAsync("add_subject", """{"name":"Programación"}""");

        Assert.True(result.Success);
        var subject = Assert.Single(store.Subjects);
        Assert.Equal("Programación", subject.Name);
        Assert.Equal("#512BD4", subject.ColorHex);
    }

    [Fact]
    public async Task UnknownTool_ReturnsFailure()
    {
        var (registry, _) = Create();

        var result = await registry.ExecuteAsync("teleport_user", "{}");

        Assert.False(result.Success);
        Assert.Contains("desconocida", result.Message);
    }

    [Fact]
    public async Task InvalidJsonArguments_ReturnFailure()
    {
        var (registry, _) = Create();

        var result = await registry.ExecuteAsync("add_subject", "{no es json");

        Assert.False(result.Success);
        Assert.Contains("JSON", result.Message);
    }
}

