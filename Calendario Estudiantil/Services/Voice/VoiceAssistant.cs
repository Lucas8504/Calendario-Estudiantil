namespace Calendario_Estudiantil.Services.Voice;

using Calendario_Estudiantil.Models;

/// <summary>
/// Orquestador del asistente de voz. No habla HTTP ni toca la base directamente:
/// delega el audio en <see cref="IGeminiClient"/> y las operaciones en <see cref="IToolRegistry"/>.
/// </summary>
public sealed class VoiceAssistant : IVoiceAssistant
{
    private const string FallbackText = "No te entendí. ¿Podés repetirlo?";

    private readonly IGeminiClient _client;
    private readonly IToolRegistry _registry;

    public VoiceAssistant(IGeminiClient client, IToolRegistry registry)
    {
        _client = client;
        _registry = registry;
    }

    public async Task<VoiceAssistantResult> ProcessAudioAsync(Stream audioStream, CancellationToken ct = default)
    {
        var response = await _client.GetVoiceCommandsAsync(
            audioStream,
            _registry.GetFunctionDeclarations(),
            DateTime.Now,
            ct);

        if (response.ToolCalls.Count == 0)
        {
            string text = string.IsNullOrWhiteSpace(response.ModelResponseText)
                ? FallbackText
                : response.ModelResponseText!;
            return new VoiceAssistantResult(new List<ToolCallResult>(), new List<PendingConfirmation>(), text);
        }

        var results = new List<ToolCallResult>();
        var pending = new List<PendingConfirmation>();

        foreach (var call in response.ToolCalls)
        {
            var result = await _registry.ExecuteAsync(call.Name, call.ArgsJson, ct);
            results.Add(result);

            if (result.RequiresConfirmation && !string.IsNullOrWhiteSpace(result.ConfirmationPrompt))
                pending.Add(new PendingConfirmation(call.Name, call.ArgsJson, result.ConfirmationPrompt));
        }

        return new VoiceAssistantResult(results, pending, null);
    }

    public async Task<VoiceAssistantResult> ConfirmAsync(IReadOnlyList<PendingConfirmation> pending, CancellationToken ct = default)
    {
        var results = new List<ToolCallResult>();

        foreach (var item in pending)
            results.Add(await _registry.ExecuteConfirmedAsync(item.ToolName, item.ArgsJson, ct));

        return new VoiceAssistantResult(results, new List<PendingConfirmation>(), null);
    }
}
