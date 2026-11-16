namespace Calendario_Estudiantil.Services.Voice;

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using Calendario_Estudiantil.Models;

/// <summary>
/// Error genérico devuelto por la API de Gemini.
/// </summary>
public sealed class GeminiRequestException : Exception
{
    public GeminiRequestException(string message) : base(message) { }
}

/// <summary>
/// Error específico cuando se excede el límite de peticiones de Gemini (HTTP 429).
/// </summary>
public sealed class GeminiRateLimitException : Exception
{
    public GeminiRateLimitException(string message) : base(message) { }
}

/// <summary>
/// Cliente HTTP hacia la API de Gemini para la capa de voz.
/// Envía audio WAV inline (base64) junto con las function declarations y
/// devuelve las function calls que el modelo decidió ejecutar.
/// </summary>
public sealed class GeminiVoiceClient : IGeminiClient
{
    // 3.8-flash venía devolviendo 503 "high demand" de forma sostenida (verificado
    // con probe real el 2026-11-14); 3.5-flash respondió 200 con audio + tools.
    private const string DefaultModel = "gemini-3.5-flash";

    private const string ModelUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/" + DefaultModel + ":generateContent";

    /// <summary>Reintentos con backoff exponencial ante errores transitorios.</summary>
    private const int MaxRetries = 3;

    private readonly HttpClient _httpClient;
    private readonly IApiKeyStore _apiKeyStore;

    public GeminiVoiceClient(HttpClient httpClient, IApiKeyStore apiKeyStore)
    {
        _httpClient = httpClient;
        _apiKeyStore = apiKeyStore;
    }

    public async Task<VoiceCommandResult> GetVoiceCommandsAsync(
        Stream audioStream,
        List<FunctionDeclaration> tools,
        DateTime today,
        string? voiceContext = null,
        CancellationToken ct = default)
    {
        string? key = await _apiKeyStore.GetGeminiApiKeyAsync();
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(
                "Falta la API key de Gemini. Configurala en Ajustes → Asistente de voz " +
                "(o definí GEMINI_API_KEY en escritorio) y volvé a intentar.");

        byte[] audioBytes = await ReadAudioBytesAsync(audioStream, ct);
        string audioBase64 = Convert.ToBase64String(audioBytes);
        string json = JsonSerializer.Serialize(
            BuildRequest(audioBase64, tools, today, voiceContext),
            new JsonSerializerOptions { WriteIndented = false });

        int attempt = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, ModelUrl);
                request.Headers.TryAddWithoutValidation("x-goog-api-key", key);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await _httpClient.SendAsync(request, ct);
                string body = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                    return GeminiResponseParser.Parse(body);

                attempt++;
                if (!IsTransient(response.StatusCode) || attempt > MaxRetries)
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        throw new GeminiRateLimitException($"Límite de peticiones excedido (429): {body}");

                    throw new GeminiRequestException($"Error de Gemini ({(int)response.StatusCode}): {body}");
                }
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                attempt++;
                await Task.Delay(RetryDelay(attempt), ct);
                continue;
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException("Gemini tardó demasiado en responder.");
            }
            catch (Exception ex) when (attempt < MaxRetries
                && !ct.IsCancellationRequested
                && IsTransportFailure(ex))
            {
                // Un reset de socket a mitad de la request es transitorio: se reintenta
                // igual que HttpRequestException. Si se agota, la excepción llega al UI
                // y MainPage la traduce al mensaje de "se cortó la conexión".
                attempt++;
                await Task.Delay(RetryDelay(attempt), ct);
                continue;
            }

            await Task.Delay(RetryDelay(attempt), ct);
        }
    }

    /// <summary>
    /// Clasifica fallos de transporte crudos. SocketException NO hereda de IOException,
    /// así que sin este guard los resets ("Connection reset", "Socket closed") no se
    /// reintentan ni se traducen en el UI. Es la fuente canónica: MainPage reutiliza
    /// esta misma regla para mapear el error que ve el usuario.
    /// </summary>
    public static bool IsTransportFailure(Exception ex)
    {
        if (ex is System.Net.Sockets.SocketException)
            return true;

        string haystack = $"{ex.GetType().Name} {ex.Message}";

        return haystack.Contains("Socket", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Connection reset", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("broken pipe", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
            || haystack.Contains("Unexpected end", StringComparison.OrdinalIgnoreCase);
    }

    private static int BackoffDelay(int attempt) => (int)(Math.Pow(2, attempt) * 1000);

    /// <summary>
    /// Delay entre reintentos en ms. Seam de test (internal): los tests lo ponen en 0
    /// para no esperar el backoff real; en producción siempre usa <see cref="BackoffDelay"/>.
    /// </summary>
    internal Func<int, int> RetryDelay { get; set; } = BackoffDelay;

    private static async Task<byte[]> ReadAudioBytesAsync(Stream audioStream, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, 81920, ct);
        return ms.ToArray();
    }

    private static object BuildRequest(string audioBase64, List<FunctionDeclaration> tools, DateTime today, string? voiceContext = null)
    {
        string todayText = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                           " (" + today.ToString("dddd", CultureInfo.InvariantCulture) + ")";

        var contextSection = string.IsNullOrWhiteSpace(voiceContext)
            ? string.Empty
            : $"\n\nCONTEXTO DE LA APLICACIÓN:\n{voiceContext.TrimEnd()}\n";

        string systemInstruction =
            $"Hoy es {todayText}.{contextSection}" +
            "Eres el asistente de voz de la app 'Calendario Estudiantil'. " +
            "Solo puedes actuar llamando a las funciones disponibles; nunca respondas con texto " +
            "salvo que el audio sea ruido o no puedas ejecutar ninguna función. " +
            "Cuando el usuario nombre una materia, pasá ese nombre tal cual en subject_name; " +
            "nunca inventes subject_id ni fechas. " +
            "Si necesitás saber qué materias existen, usá find_subject. " +
            "Las fechas van en ISO 8601 (ej. 2026-11-13T18:00:00) y las horas en HH:mm. " +
            "Las operaciones de edición y borrado las confirma la app, así que no pidas confirmación por texto.";

        var audioPart = new { inlineData = new { mimeType = "audio/wav", data = audioBase64 } };
        var contents = new[] { new { role = "user", parts = new object[] { audioPart } } };

        var functionDeclarations = tools
            .Select(t => new
            {
                name = t.Name,
                description = t.Description,
                parameters = JsonSerializer.Deserialize<JsonElement>(t.ParametersJson)
            })
            .ToArray();

        return new
        {
            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
            contents,
            tools = new[] { new { functionDeclarations } },
            toolConfig = new { functionCallingConfig = new { mode = "AUTO" } },
            generationConfig = new { temperature = 0.2 }
        };
    }

    /// <summary>
    /// ¿Vale la pena reintentar esta respuesta? (timeouts y errores de servidor)
    /// </summary>
    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.RequestTimeout ||
        status == HttpStatusCode.BadGateway ||
        status == HttpStatusCode.ServiceUnavailable ||
        status == HttpStatusCode.GatewayTimeout;
}

