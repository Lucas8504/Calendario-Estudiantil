namespace Calendario_Estudiantil.Services.Voice;

using Calendario_Estudiantil.Models;

/// <summary>
/// Cliente HTTP minimalista hacia Gemini. Solo envuelve el protocolo de red:
/// audio inline (base64) + system instruction + function declarations (tools)
/// + parseo de las function calls devueltas por el modelo.
/// No tiene dependencias con los servicios de negocio.
/// </summary>
public interface IGeminiClient
{
    /// <summary>
    /// Envía el audio al modelo y devuelve las llamadas a funciones que Gemini decidió
    /// ejecutar (o el texto de respuesta si no llamó a ninguna función).
    /// </summary>
    /// <param name="audioStream">Stream de audio WAV PCM mono 16 kHz (Mono, 16 bits).</param>
    /// <param name="tools">Declaraciones de las funciones disponibles para la IA.</param>
    /// <param name="today">Hoy (para que la IA resuelva "mañana"/"el viernes").</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Las function calls de Gemini (o texto si no llamó a ninguna función).</returns>
    /// <exception cref="InvalidOperationException">Si GEMINI_API_KEY no está configurada.</exception>
    Task<VoiceCommandResult> GetVoiceCommandsAsync(
        Stream audioStream,
        List<FunctionDeclaration> tools,
        DateTime today,
        CancellationToken ct = default);
}
