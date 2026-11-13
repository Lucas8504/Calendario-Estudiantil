using Xunit;
using Calendario_Estudiantil.Services.Voice;

namespace Calendario_Estudiantil.Voice.Tests;

public class GeminiResponseParserTests
{
    [Fact]
    public void FunctionCall_ReturnsToolCallWithRawJsonArgs()
    {
        const string json = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "functionCall": { "name": "add_task", "args": { "title": "TP", "due_date": "2026-12-01T18:00:00" } } }
                ]
              }
            }
          ]
        }
        """;

        var result = GeminiResponseParser.Parse(json);

        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("add_task", call.Name);
        Assert.Null(result.ModelResponseText);

        using var args = System.Text.Json.JsonDocument.Parse(call.ArgsJson);
        Assert.Equal("TP", args.RootElement.GetProperty("title").GetString());
        Assert.Equal("2026-12-01T18:00:00", args.RootElement.GetProperty("due_date").GetString());
    }

    [Fact]
    public void MultipleFunctionCalls_AreAllReturned()
    {
        const string json = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "functionCall": { "name": "add_subject", "args": { "name": "Física" } } },
                  { "functionCall": { "name": "add_task", "args": { "title": "TP" } } }
                ]
              }
            }
          ]
        }
        """;

        var result = GeminiResponseParser.Parse(json);

        Assert.Equal(2, result.ToolCalls.Count);
        Assert.Equal("add_subject", result.ToolCalls[0].Name);
        Assert.Equal("add_task", result.ToolCalls[1].Name);
    }

    [Fact]
    public void FunctionCallWithoutArgs_UsesEmptyObject()
    {
        const string json = """
        { "candidates": [ { "content": { "parts": [ { "functionCall": { "name": "find_subject" } } ] } } ] }
        """;

        var result = GeminiResponseParser.Parse(json);

        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("{}", call.ArgsJson);
    }

    [Fact]
    public void StringArgs_ArePassedThrough()
    {
        const string json = """
        { "candidates": [ { "content": { "parts": [ { "functionCall": { "name": "add_subject", "args": "{\"name\":\"Física\"}" } } ] } } ] }
        """;

        var result = GeminiResponseParser.Parse(json);

        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("{\"name\":\"Física\"}", call.ArgsJson);
    }

    [Fact]
    public void TextOnlyResponse_ReturnsModelText()
    {
        const string json = """
        { "candidates": [ { "content": { "parts": [ { "text": "No escuché nada." } ] } } ] }
        """;

        var result = GeminiResponseParser.Parse(json);

        Assert.Empty(result.ToolCalls);
        Assert.Equal("No escuché nada.", result.ModelResponseText);
    }

    [Fact]
    public void EmptyParts_FallBackToDefaultText()
    {
        const string json = """
        { "candidates": [ { "content": { "parts": [] } } ] }
        """;

        var result = GeminiResponseParser.Parse(json);

        Assert.Empty(result.ToolCalls);
        Assert.Equal(GeminiResponseParser.FallbackText, result.ModelResponseText);
    }

    [Fact]
    public void MissingCandidates_ReturnsNoCandidatesMessage()
    {
        const string json = """{ "usageMetadata": { } }""";

        var result = GeminiResponseParser.Parse(json);

        Assert.Empty(result.ToolCalls);
        Assert.Equal(GeminiResponseParser.NoCandidatesMessage, result.ModelResponseText);
    }

    [Fact]
    public void ContentWithoutParts_ReturnsNoPartsMessage()
    {
        const string json = """
        { "candidates": [ { "content": { "role": "model" } } ] }
        """;

        var result = GeminiResponseParser.Parse(json);

        Assert.Empty(result.ToolCalls);
        Assert.Equal(GeminiResponseParser.NoPartsMessage, result.ModelResponseText);
    }

    [Fact]
    public void SafetyBlock_ReturnsBlockReasonMessage()
    {
        const string json = """
        { "promptFeedback": { "blockReason": "SAFETY" } }
        """;

        var result = GeminiResponseParser.Parse(json);

        Assert.Empty(result.ToolCalls);
        Assert.Contains("bloqueado", result.ModelResponseText);
        Assert.Contains("SAFETY", result.ModelResponseText);
    }

    [Fact]
    public void MalformedJson_ReturnsMalformedMessage()
    {
        var result = GeminiResponseParser.Parse("{no es json");

        Assert.Empty(result.ToolCalls);
        Assert.Equal(GeminiResponseParser.MalformedJsonMessage, result.ModelResponseText);
    }
}

