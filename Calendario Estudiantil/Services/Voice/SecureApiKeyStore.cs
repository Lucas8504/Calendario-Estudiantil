namespace Calendario_Estudiantil.Services.Voice;

/// <summary>
/// Implementación sobre <see cref="SecureStorage"/>: la key queda cifrada en el
/// dispositivo (Keystore en Android, Keychain en iOS/MacCatalyst, DPAPI en Windows).
/// </summary>
public sealed class SecureApiKeyStore : IApiKeyStore
{
    private const string GeminiKeyName = "gemini_api_key";

    public async Task<string?> GetGeminiApiKeyAsync()
    {
        string? stored = null;

        try
        {
            stored = await SecureStorage.Default.GetAsync(GeminiKeyName);
        }
        catch (Exception ex)
        {
            // SecureStorage puede fallar si el almacén del sistema no está disponible.
            System.Diagnostics.Debug.WriteLine($"No se pudo leer la API key guardada: {ex.Message}");
        }

        return ApiKeyResolver.Resolve(stored, Environment.GetEnvironmentVariable("GEMINI_API_KEY"));
    }

    public async Task SetGeminiApiKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La API key no puede estar vacía.", nameof(key));

        await SecureStorage.Default.SetAsync(GeminiKeyName, key.Trim());
    }

    public Task ClearGeminiApiKeyAsync()
    {
        SecureStorage.Default.Remove(GeminiKeyName);
        return Task.CompletedTask;
    }
}
