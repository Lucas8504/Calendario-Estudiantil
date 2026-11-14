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

    // El STT de Gemini suele devolver el nombre como una sola palabra sin separadores.
    [Theory]
    [InlineData("Fisicoquimica")]
    [InlineData("fisicoquimica")]
    [InlineData("FÍSICOQUIMICA")]
    public async Task FindSubject_JoinedQuery_MatchesSeparatedSubject(string query)
    {
        var (registry, store) = Create();
        store.AddSubject("Físico-Química");

        var result = await registry.ExecuteAsync("find_subject", $$"""{"name":"{{query}}"}""");

        Assert.True(result.Success);
        Assert.Contains("Físico-Química", result.Message);
    }

    [Fact]
    public async Task FindSubject_SeparatedQuery_MatchesJoinedSubject()
    {
        var (registry, store) = Create();
        store.AddSubject("Fisicoquimica");

        var result = await registry.ExecuteAsync("find_subject", """{"name":"Físico Química"}""");

        Assert.True(result.Success);
        Assert.Contains("Fisicoquimica", result.Message);
    }

    [Fact]
    public async Task AddTask_JoinedSubjectName_ResolvesHyphenatedSubject()
    {
        var (registry, store) = Create();
        var subject = store.AddSubject("Físico-Química");

        var result = await registry.ExecuteAsync("add_task",
            """{"title":"Informe de laboratorio","subject_name":"Fisicoquimica","due_date":"2026-12-01T18:00:00"}""");

        Assert.True(result.Success);
        Assert.Equal(subject.Id, Assert.Single(store.Tasks).SubjectId);
    }
}
