namespace Calendario_Estudiantil.Services.Voice;

using System.Text.Json;

using Calendario_Estudiantil.Models;

/// <summary>
/// Parser de la respuesta de Gemini: convierte el JSON de <c>generateContent</c>
/// en las function calls que el modelo pidió ejecutar (o en el texto de respuesta).
/// Aislado del cliente HTTP para poder testearlo sin red.
/// </summary>
public static class GeminiResponseParser
{
    public const string MalformedJsonMessage = "La respuesta de Gemini no es JSON válido.";
    public const string NoCandidatesMessage = "La respuesta de Gemini no tiene candidatos válidos.";
    public const string NoPartsMessage = "La respuesta de Gemini no contiene partes.";
    public const string FallbackText = "No te entendí. ¿Podés repetirlo?";

    /// <summary>
    /// Parsea la respuesta cruda del modelo.
    /// </summary>
    /// <param name="json">Cuerpo JSON devuelto por <c>generateContent</c>.</param>
    public static VoiceCommandResult Parse(string json)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return new VoiceCommandResult(new List<ToolCall>(), MalformedJsonMessage);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.TryGetProperty("promptFeedback", out var feedback)
                && feedback.TryGetProperty("blockReason", out var blockReason)
                && blockReason.ValueKind == JsonValueKind.String)
            {
                return new VoiceCommandResult(new List<ToolCall>(),
                    $"El audio fue bloqueado por los filtros de seguridad ({blockReason.GetString()}). Probá decirlo de otra forma.");
            }

            if (!root.TryGetProperty("candidates", out var candidates)
                || candidates.ValueKind != JsonValueKind.Array
                || candidates.GetArrayLength() == 0)
            {
                return new VoiceCommandResult(new List<ToolCall>(), NoCandidatesMessage);
            }

            var candidate = candidates[0];
            if (!candidate.TryGetProperty("content", out var content)
                || !content.TryGetProperty("parts", out var parts)
                || parts.ValueKind != JsonValueKind.Array)
            {
                return new VoiceCommandResult(new List<ToolCall>(), NoPartsMessage);
            }

            var calls = new List<ToolCall>();
            string? text = null;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("functionCall", out var functionCall)
                    && functionCall.TryGetProperty("name", out var nameElement)
                    && nameElement.ValueKind == JsonValueKind.String)
                {
                    string name = nameElement.GetString()!;
                    string args = functionCall.TryGetProperty("args", out var argsElement)
                        ? argsElement.ValueKind == JsonValueKind.String
                            ? argsElement.GetString() ?? "{}"
                            : argsElement.GetRawText()
                        : "{}";

                    calls.Add(new ToolCall(name, args));
                }
                else if (part.TryGetProperty("text", out var textElement)
                         && textElement.ValueKind == JsonValueKind.String)
                {
                    text = textElement.GetString();
                }
            }

            if (calls.Count > 0)
                return new VoiceCommandResult(calls, null);

            return new VoiceCommandResult(
                new List<ToolCall>(),
                string.IsNullOrWhiteSpace(text) ? FallbackText : text);
        }
    }
}
