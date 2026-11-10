namespace Calendario_Estudiantil.Models;

/// <summary>
/// Resultado de llamar a Gemini con audio: puede devolver múltiples llamadas a funciones
/// (function calls) en una sola respuesta, o texto si no llamó a ninguna función.
/// </summary>
public record VoiceCommandResult(List<ToolCall> ToolCalls, string? ModelResponseText);

/// <summary>
/// Una llamada a una herramienta devuelta por Gemini.
/// </summary>
public record ToolCall(string Name, string ArgsJson);
