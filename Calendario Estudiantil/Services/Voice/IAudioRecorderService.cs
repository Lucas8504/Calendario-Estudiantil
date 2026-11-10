namespace Calendario_Estudiantil.Services.Voice;

using Plugin.Maui.Audio;

/// <summary>
/// Grabación de audio para el asistente de voz.
/// Graba WAV PCM mono 16 kHz (formato que Gemini acepta inline).
/// </summary>
public interface IAudioRecorderService
{
    /// <summary>True mientras hay una grabación en curso.</summary>
    bool IsRecording { get; }

    /// <summary>Pide permiso de micrófono si hace falta y empieza a grabar.</summary>
    Task StartAsync();

    /// <summary>
    /// Detiene la grabación y devuelve el stream con el audio grabado,
    /// o null si no había una grabación activa.
    /// </summary>
    Task<Stream?> StopAsync();

    /// <summary>Descarta la grabación actual sin devolver audio.</summary>
    Task CancelAsync();
}

/// <summary>
/// Implementación sobre Plugin.Maui.Audio.
/// </summary>
public sealed class AudioRecorderService : IAudioRecorderService
{
    private static readonly AudioRecorderOptions RecorderOptions = new()
    {
        SampleRate = 16000,
        Channels = ChannelType.Mono,
        BitDepth = BitDepth.Pcm16bit,
        Encoding = Plugin.Maui.Audio.Encoding.Wav,
        ThrowIfNotSupported = false
    };

    private readonly IAudioManager _audioManager;
    private IAudioRecorder? _recorder;

    public AudioRecorderService(IAudioManager audioManager) => _audioManager = audioManager;

    public bool IsRecording => _recorder?.IsRecording ?? false;

    public async Task StartAsync()
    {
        if (IsRecording)
            return;

        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status == PermissionStatus.Denied)
            throw new InvalidOperationException(
                "No hay permiso para usar el micrófono. Habilitalo en los ajustes del sistema.");

        _recorder = _audioManager.CreateRecorder(RecorderOptions);
        if (!_recorder.CanRecordAudio)
            throw new InvalidOperationException(
                "No pude acceder al micrófono. Verificá que esté disponible y que la app tenga permiso.");

        await _recorder.StartAsync(RecorderOptions);
    }

    public async Task<Stream?> StopAsync()
    {
        if (_recorder is null || !_recorder.IsRecording)
        {
            _recorder = null;
            return null;
        }

        var source = await _recorder.StopAsync();
        _recorder = null;
        return source.GetAudioStream();
    }

    public async Task CancelAsync()
    {
        if (_recorder is null)
            return;

        try
        {
            if (_recorder.IsRecording)
                await _recorder.StopAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cancelando grabación: {ex.Message}");
        }
        finally
        {
            _recorder = null;
        }
    }
}
