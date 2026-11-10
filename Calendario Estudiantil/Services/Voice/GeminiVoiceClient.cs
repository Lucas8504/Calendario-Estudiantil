namespace Calendario_Estudiantil.Services.Voice;

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using Calendario_Estudiantil.Models;

/// <summary>
/// Errores que lanza el cliente cuando Gemini responde con error HTTP.
/// </summary>
public sealed class GeminiRequestException : Exception
{
    public GeminiRequestException(string message) : base(message) { }
}

/// <summary>
/// Excepción específica cuando se excede el límite de peticiones de Gemini.
/// </summary>
public sealed class GeminiRateLimitException : Exception
{
    public GeminiRateLimitException(string message) : base(message) { }
}

/// <summary>
/// Cliente HTTP hacia la API de Gemini para la capa de voz.
/// </summary>
public sealed class GeminiVoiceClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Número de reintentos (backoff exponencial) ante 429/5xx.
    /// </summary>
    private const int MaxRetries = 3;

    public GeminiVoiceClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<VoiceCommandResult> GetVoiceCommandsAsync(
        Stream audioStream,
        List<FunctionDeclaration> tools,
        DateTime today,
        CancellationToken ct = default)
    {
        string? key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (key is null)
            throw new InvalidOperationException(
                "Falta la variable de entorno GEMINI_API_KEY. Configurala en tu entorno local " +
                "(nunca se commita la clave) y vuelve a intentar.");

        byte[] audioBytes = await ReadAudioBytesAsync(audioStream, ct);
        string audioBase64 = Convert.ToBase64String(audioBytes);

        var requestBody = BuildRequest(audioBase64, tools, today);
        string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = false });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        int attempt = 0;
        while (true)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content, ct);
                if (response.IsSuccessStatusCode)
                    return ParseResponse(await response.Content.ReadAsStringAsync(ct));

                attempt++;
                if (!IsTransient(response.StatusCode) || attempt >= MaxRetries)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(ct);
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        throw new GeminiRateLimitException($"Límite de peticiones excedido ({response.StatusCode}): {errorBody}");
                    throw new GeminiRequestException($"Error de Gemini ({response.StatusCode}): {errorBody}");
                }

                int delayMs = (int)(Math.Pow(2, attempt) * 1000); // 2s, 4s, 8s...
                await Task.Delay(delayMs, ct);
            }
            catch (HttpRequestException ex) when (attempt++ < MaxRetries && !ct.IsCancellationRequested)
            {
                await Task.Delay((int)(Math.Pow(2, attempt) * 1000), ct);
                continue;
            }
        }
    }

    private async Task<byte[]> ReadAudioBytesAsync(Stream audioStream, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await audioStream.CopyToAsync(ms, 81920, ct);
        return ms.ToArray();
    }

    private object BuildRequest(string audioBase64, List<FunctionDeclaration> tools, DateTime today)
    {
        string todayText = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                           " (" + today.ToString("dddd", CultureInfo.InvariantCulture) + ")";

        string systemInstruction =
            "Hoy es " + todayText + ". ";
        systemInstruction +=
            "Eres el asistente de voz de la app 'Calendario Estudiantil'. " +
            "Solo puedes actuar respondiendo con llamadas a las funciones disponibles. " +
            "Nunca respondas con texto salvo que el audio sea ruido o no puedas ejecutar ninguna función. " +
            "Si no alcanzás a entender, devolvé un tool_call con 'find_subject' y el nombre tal cual lo escuchaste, " +
            "sin inventar materias ni fechas.";

        var systemPart = new { text = systemInstruction };
        var audioPart = new { inlineData = new { mimeType = "audio/wav", data = audioBase64 } };
        var content = new[] { new { parts = new object[] { systemPart, audioPart } } };

        var functionDeclarations = tools
            .Select(t => JsonSerializer.Deserialize<Dictionary<string, object>>(t.ParametersJson, JsonOptions)!)
            .ToArray();
        var toolsPayload = new[] { new { functionDeclarations } };

        return new
        {
            contents = content,
            tools = toolsPayload,
            toolConfig = new { functionCallingConfig = new { mode = "AUTO" } }
        };
    }

    private VoiceCommandResult ParseResponse(string json)
    {
        var root = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);
        var candidates = root?["candidates"] as List<object>;
        var choices = root?["choices"] as List<object>;

        object? content = null;
        if (candidates != null && candidates.Count > 0 && candidates[0] is Dictionary<string, object> candidate)
            content = candidate["content"];
        else if (choices != null && choices.Count > 0 && choices[0] is Dictionary<string, object> choice)
        {
            content = choice.TryGetValue("message", out var messageObj) && messageObj is Dictionary<string, object> message
                ? message["content"]
                : null;
        }

        if (content is not Dictionary<string, object> contentDict)
            return new VoiceCommandResult(new List<ToolCall>(), "La respuesta de Gemini no tiene una estructura válida.");

        var parts = contentDict["parts"] as List<object>;
        if (parts != null && parts.Count > 0)
        {
            var calls = new List<ToolCall>();
            string? text = null;

            foreach (var part in parts)
            {
                var dict = (Dictionary<string, object>)part;
                if (dict.TryGetValue("functionCall", out var fc) && fc is Dictionary<string, object> fcDict)
                {
                    calls.Add(new ToolCall(
                        (string?)fcDict["name"]!,
                        (string?)fcDict["args"] ?? "{}"));
                }
                else if (dict.TryGetValue("text", out var t))
                {
                    text = (string?)t;
                }
            }

            if (calls.Count > 0)
                return new VoiceCommandResult(calls, null);
            return new VoiceCommandResult(new List<ToolCall>(), text);
        }

        return new VoiceCommandResult(new List<ToolCall>(), "La respuesta de Gemini no contiene partes.");
    }

    /// <summary>
    /// ¿Se puede reintentar esta respuesta sin lanzar inmediatamente?
    /// </summary>
    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.RequestTimeout ||
        status == HttpStatusCode.BadGateway ||
        status == HttpStatusCode.ServiceUnavailable ||
        status == HttpStatusCode.GatewayTimeout;
}
