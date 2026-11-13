using Xunit;
using Calendario_Estudiantil.Models;
using Calendario_Estudiantil.Services.Voice;
using Calendario_Estudiantil.Voice.Tests.Fakes;

namespace Calendario_Estudiantil.Voice.Tests;

public class VoiceAssistantTests
{
    private static (VoiceAssistant Assistant, FakeGeminiClient Client, InMemoryStore Store) Create()
    {
        var store = new InMemoryStore();
        var registry = new ToolRegistry(
            new FakeTaskService(store),
            new FakeSubjectService(store),
            new FakeDatabaseService(store));
        var client = new FakeGeminiClient();
        return (new VoiceAssistant(client, registry), client, store);
    }

    [Fact]
    public async Task NoToolCalls_ReturnsModelText()
    {
        var (assistant, client, _) = Create();
        client.Result = new VoiceCommandResult(new List<ToolCall>(), "Hola, ¿qué querés hacer?");

        var result = await assistant.ProcessAudioAsync(Stream.Null);

        Assert.Equal("Hola, ¿qué querés hacer?", result.AssistantText);
        Assert.Empty(result.Results);
        Assert.Empty(result.PendingConfirmations);
    }

    [Fact]
    public async Task NoToolCallsAndBlankText_ReturnsFallback()
    {
        var (assistant, client, _) = Create();
        client.Result = new VoiceCommandResult(new List<ToolCall>(), "   ");

        var result = await assistant.ProcessAudioAsync(Stream.Null);

        Assert.Equal("No te entendí. ¿Podés repetirlo?", result.AssistantText);
    }

    [Fact]
    public async Task ToolCall_ExecutesImmediatelyAndExposesPendingConfirmation()
    {
        var (assistant, client, store) = Create();
        var subject = store.AddSubject("Lengua");
        var task = store.AddTask(subject.Id, "Leer", new DateTime(2026, 11, 15, 9, 0, 0));
        client.Result = new VoiceCommandResult(
            new List<ToolCall> { new("delete_task", $$"""{"task_id":{{task.Id}}}""") },
            null);

        var result = await assistant.ProcessAudioAsync(Stream.Null);

        var toolResult = Assert.Single(result.Results);
        Assert.True(toolResult.RequiresConfirmation);

        var pending = Assert.Single(result.PendingConfirmations);
        Assert.Equal("delete_task", pending.ToolName);
        Assert.Contains("borrar", pending.Prompt);
        Assert.Single(store.Tasks);
    }

    [Fact]
    public async Task ConfirmAsync_ExecutesOnlyAcceptedOperations()
    {
        var (assistant, client, store) = Create();
        var subject = store.AddSubject("Lengua");
        var task = store.AddTask(subject.Id, "Leer", new DateTime(2026, 11, 15, 9, 0, 0));
        client.Result = new VoiceCommandResult(
            new List<ToolCall> { new("delete_task", $$"""{"task_id":{{task.Id}}}""") },
            null);

        var first = await assistant.ProcessAudioAsync(Stream.Null);
        var confirmed = await assistant.ConfirmAsync(first.PendingConfirmations);

        var toolResult = Assert.Single(confirmed.Results);
        Assert.True(toolResult.Success);
        Assert.Empty(store.Tasks);
        Assert.Empty(confirmed.PendingConfirmations);
    }

    [Fact]
    public async Task ProcessAudioAsync_PassesToolDeclarationsToClient()
    {
        var (assistant, client, _) = Create();
        client.Result = new VoiceCommandResult(new List<ToolCall>(), "ok");

        await assistant.ProcessAudioAsync(Stream.Null);

        Assert.Equal(1, client.CallCount);
        Assert.NotNull(client.LastTools);
        Assert.Equal(10, client.LastTools!.Count);
    }
}

