namespace Calendario_Estudiantil.Services.Voice;

using Calendario_Estudiantil.Models;

/// <summary>
/// Orquestador del asistente de voz: toma audio, lo manda a la IA, ejecuta las
/// herramientas que el modelo pidió y devuelve el resultado para que la UI lo muestre.
/// </summary>
public interface IVoiceAssistant
{
    /// <summary>
    /// Procesa un audio del usuario: transcribe, interpreta y ejecuta las operaciones
    /// no destructivas. Las operaciones que requieren confirmación se devuelven en
    /// <see cref="VoiceAssistantResult.PendingConfirmations"/>.
    /// </summary>
    Task<VoiceAssistantResult> ProcessAudioAsync(Stream audioStream, CancellationToken ct = default);

    /// <summary>
    /// Ejecuta las operaciones que quedaron pendientes de confirmación del usuario.
    /// </summary>
    Task<VoiceAssistantResult> ConfirmAsync(IReadOnlyList<PendingConfirmation> pending, CancellationToken ct = default);
}
