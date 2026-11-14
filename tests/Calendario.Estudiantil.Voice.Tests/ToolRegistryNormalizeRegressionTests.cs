using Xunit;
using Calendario_Estudiantil.Services.Voice;
using Calendario_Estudiantil.Voice.Tests.Fakes;

namespace Calendario_Estudiantil.Voice.Tests;

/// <summary>
/// Regresiones del colapso de separadores en Normalize: una entrada que solo tiene
/// separadores no debe normalizar a vacío, porque vacío matchea cualquier materia.
/// </summary>
public class ToolRegistryNormalizeRegressionTests
{
    private static (ToolRegistry Registry, InMemoryStore Store) Create()
    {
        var store = new InMemoryStore();
        return (new ToolRegistry(
            new FakeTaskService(store),
            new FakeSubjectService(store),
            new FakeDatabaseService(store)), store);
    }

    [Theory]
    [InlineData("-")]
    [InlineData(".")]
    [InlineData("¿?")]
    [InlineData("///")]
    [InlineData("\u2013")]
    public async Task FindSubject_SeparatorOnlyQuery_Fails(string query)
    {
        var (registry, store) = Create();
        store.AddSubject("Física");
        store.AddSubject("Matemática");

        var result = await registry.ExecuteAsync("find_subject", $$"""{"name":"{{query}}"}""");

        Assert.False(result.Success);
    }

    [Theory]
    [InlineData("-")]
    [InlineData("...")]
    public async Task AddTask_SeparatorOnlySubjectName_Fails(string spoken)
    {
        var (registry, store) = Create();
        store.AddSubject("Física");

        var result = await registry.ExecuteAsync("add_task",
            $$"""{"title":"X","subject_name":"{{spoken}}","due_date":"2026-12-01T18:00:00"}""");

        Assert.False(result.Success);
        Assert.Empty(store.Tasks);
    }

    [Theory]
    [InlineData("-")]
    [InlineData("--")]
    public async Task AddSchedule_SeparatorOnlySubjectName_Fails(string spoken)
    {
        var (registry, store) = Create();
        store.AddSubject("Física");

        var result = await registry.ExecuteAsync("add_schedule",
            $$"""{"subject_name":"{{spoken}}","day":3,"start_time":"18:00","end_time":"20:00"}""");

        Assert.False(result.Success);
        Assert.Empty(store.Schedules);
    }
}
