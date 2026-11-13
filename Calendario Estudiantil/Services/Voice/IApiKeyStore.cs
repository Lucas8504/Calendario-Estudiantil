namespace Calendario_Estudiantil.Services.Voice;

/// <summary>
/// Acceso a la API key de Gemini. En móvil las variables de entorno del host
/// no llegan al proceso de la app, así que la key se guarda en el almacén
/// seguro del dispositivo; en escritorio se mantiene el fallback por variable
/// de entorno para desarrollo.
/// </summary>
public interface IApiKeyStore
{
    /// <summary>Devuelve la key configurada, o null si no hay ninguna.</summary>
    Task<string?> GetGeminiApiKeyAsync();

    /// <summary>Guarda la key en el almacén seguro del dispositivo.</summary>
    Task SetGeminiApiKeyAsync(string key);

    /// <summary>Borra la key guardada.</summary>
    Task ClearGeminiApiKeyAsync();
}

/// <summary>
/// Reglas puras de resolución de la key (sin dependencias de MAUI), para poder testearlas.
/// </summary>
public static class ApiKeyResolver
{
    /// <summary>
    /// Prioriza la key guardada en el dispositivo y cae a la variable de entorno.
    /// Ignora valores vacíos o con solo espacios.
    /// </summary>
    public static string? Resolve(string? storedKey, string? environmentValue)
        => FirstNonEmpty(storedKey, environmentValue);

    private static string? FirstNonEmpty(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
                return candidate.Trim();
        }

        return null;
    }
}
