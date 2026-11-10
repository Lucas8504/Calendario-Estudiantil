namespace Calendario_Estudiantil.Models;

/// <summary>
/// Resultado de ejecutar una herramienta del asistente de voz.
/// </summary>
/// <param name="ToolName">Nombre de la función ejecutada.</param>
/// <param name="Success">Si la operación terminó bien.</param>
/// <param name="Message">Mensaje para mostrarle al usuario.</param>
/// <param name="DataJson">Datos opcionales (JSON) que la herramienta quiso devolver.</param>
/// <param name="RequiresConfirmation">True si la operación necesita confirmación explícita del usuario.</param>
/// <param name="ConfirmationPrompt">Texto de la confirmación (solo si RequiresConfirmation).</param>
public record ToolCallResult(
    string ToolName,
    bool Success,
    string? Message,
    string? DataJson,
    bool RequiresConfirmation,
    string? ConfirmationPrompt)
{
    public static ToolCallResult Ok(string toolName, string message, string? dataJson = null)
        => new(toolName, true, message, dataJson, false, null);

    public static ToolCallResult Failure(string toolName, string message)
        => new(toolName, false, message, null, false, null);

    public static ToolCallResult NeedsConfirmation(string toolName, string prompt)
        => new(toolName, false, null, null, true, prompt);
}

/// <summary>
/// Operación que quedó pendiente de confirmación del usuario antes de ejecutarse.
/// </summary>
public record PendingConfirmation(string ToolName, string ArgsJson, string Prompt);

/// <summary>
/// Resultado de un turno del asistente de voz.
/// </summary>
/// <param name="Results">Resultados de las herramientas ejecutadas inmediatamente.</param>
/// <param name="PendingConfirmations">Operaciones que esperan confirmación del usuario.</param>
/// <param name="AssistantText">Texto libre devuelto por el modelo (si no llamó a ninguna herramienta).</param>
public record VoiceAssistantResult(
    List<ToolCallResult> Results,
    List<PendingConfirmation> PendingConfirmations,
    string? AssistantText);
