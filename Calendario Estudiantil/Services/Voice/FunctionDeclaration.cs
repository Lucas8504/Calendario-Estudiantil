namespace Calendario_Estudiantil.Services.Voice;

using System.Text.Json;

/// <summary>
/// Declaración de una función (tool) que Gemini puede llamar.
/// </summary>
/// <param name="Name">Nombre de la función (se usa como identificador de dispatch).</param>
/// <param name="Description">Descripción en lenguaje natural para que la IA entienda cuándo invocar.</param>
/// <param name="ParametersJson">JSON con el schema de los parámetros (objeto JSON con 'type' y 'properties').</param>
public record FunctionDeclaration(string Name, string Description, string ParametersJson);

/// <summary>
/// Ayudas para construir schemas JSON compatibles con Gemini function calling.
/// </summary>
public static class FunctionDeclarationBuilder
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Crea un schema de función con un conjunto de propiedades simples.
    /// </summary>
    /// <param name="name">Nombre de la función.</param>
    /// <param name="description">Descripción natural de la función.</param>
    /// <param name="properties">Diccionario: nombre de propiedad -> { type: string, description: string? }. type soporta: string, int, number, bool, datetime (ISO 8601), time (HH:mm), enum (array de strings).</param>
    /// <param name="required">Propiedades obligatorias.</param>
    public static FunctionDeclaration Create(
        string name,
        string description,
        Dictionary<string, object> properties,
        List<string>? required = null)
    {
        var schema = new
        {
            type = "object",
            properties = properties,
            required = required ?? new List<string>()
        };
        var parameters = JsonSerializer.Serialize(schema, Options);
        return new FunctionDeclaration(name, description, parameters);
    }

    /// <summary>
    /// Declara una propiedad de tipo cadena.
    /// </summary>
    public static Dictionary<string, object> StringProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "string" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad de tipo número entero.
    /// </summary>
    public static Dictionary<string, object> IntProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "integer" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad de tipo número decimal.
    /// </summary>
    public static Dictionary<string, object> NumberProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "number" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad booleana.
    /// </summary>
    public static Dictionary<string, object> BoolProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "boolean" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad datetime (valor ISO 8601 en strings, ej. 2026-11-10T18:00:00).
    /// </summary>
    public static Dictionary<string, object> DateTimeProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "string" },
            { "format", "date-time" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad time (valor HH:mm en strings, ej. 08:00).
    /// </summary>
    public static Dictionary<string, object> TimeProperty(string name, string description, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "string" },
            { "format", "HH:mm" },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }

    /// <summary>
    /// Declara una propiedad enumerada (valores pasados como strings).
    /// </summary>
    public static Dictionary<string, object> EnumProperty(string name, string description, string[] values, bool required)
    {
        var prop = new Dictionary<string, object>
        {
            { "type", "string" },
            { "enum", values },
            { "description", description }
        };
        if (required) prop.Add("required", true);
        return prop;
    }
}
