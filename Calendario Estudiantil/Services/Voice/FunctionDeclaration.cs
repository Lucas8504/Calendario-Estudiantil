namespace Calendario_Estudiantil.Services.Voice;

using System.Text.Json;

/// <summary>
/// Declaración de una función (tool) que Gemini puede llamar.
/// </summary>
/// <param name="Name">Nombre de la función (identificador de dispatch).</param>
/// <param name="Description">Descripción en lenguaje natural para que la IA entienda cuándo invocar.</param>
/// <param name="ParametersJson">JSON con el schema de parámetros (objeto con 'type', 'properties' y 'required').</param>
public record FunctionDeclaration(string Name, string Description, string ParametersJson);

/// <summary>
/// Construye schemas de parámetros compatibles con Gemini function calling.
/// </summary>
public static class FunctionDeclarationBuilder
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    /// <summary>
    /// Crea una declaración de función con sus propiedades y las obligatorias.
    /// </summary>
    /// <param name="name">Nombre de la función.</param>
    /// <param name="description">Descripción natural.</param>
    /// <param name="properties">Mapa nombre -> schema de la propiedad (usar <see cref="Prop"/>).</param>
    /// <param name="required">Nombres de propiedades obligatorias.</param>
    public static FunctionDeclaration Create(
        string name,
        string description,
        Dictionary<string, object> properties,
        params string[] required)
    {
        var schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required
        };
        return new FunctionDeclaration(name, description, JsonSerializer.Serialize(schema, Options));
    }

    /// <summary>
    /// Schema de una propiedad simple. Tipos válidos: string, integer, number, boolean.
    /// </summary>
    public static Dictionary<string, object> Prop(string type, string description)
        => new() { ["type"] = type, ["description"] = description };

    /// <summary>
    /// Schema de una propiedad enumerada (strings).
    /// </summary>
    public static Dictionary<string, object> EnumProp(string description, params string[] values)
        => new() { ["type"] = "string", ["description"] = description, ["enum"] = values };
}
