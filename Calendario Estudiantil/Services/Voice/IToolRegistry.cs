namespace Calendario_Estudiantil.Services.Voice;

using Calendario_Estudiantil.Models;

/// <summary>
/// Registro de herramientas (tools) del asistente de voz: mantiene el schema que se le
/// pasa a Gemini y el handler que ejecuta cada operación sobre los servicios de la app.
/// </summary>
public interface IToolRegistry
{
    /// <summary>Declaraciones de todas las herramientas disponibles.</summary>
    List<FunctionDeclaration> GetFunctionDeclarations();

    /// <summary>
    /// Ejecuta una herramienta. Las operaciones destructivas o de edición NO se ejecutan
    /// en esta llamada: devuelven <see cref="ToolCallResult.RequiresConfirmation"/> = true.
    /// </summary>
    Task<ToolCallResult> ExecuteAsync(string toolName, string argsJson, CancellationToken ct = default);

    /// <summary>
    /// Ejecuta una herramienta ya confirmada por el usuario (segunda fase de una operación
    /// que devolvió RequiresConfirmation).
    /// </summary>
    Task<ToolCallResult> ExecuteConfirmedAsync(string toolName, string argsJson, CancellationToken ct = default);

    /// <summary>
    /// Devuelve un resumen legible del estado actual de la app (materias, tareas
    /// pendientes y horarios) para inyectarlo como contexto en el system prompt de Gemini.
    /// Así la IA puede resolver nombres aproximados contra lo que realmente está cargado.
    /// </summary>
    Task<string> GetVoiceContextAsync(CancellationToken ct = default);
}
