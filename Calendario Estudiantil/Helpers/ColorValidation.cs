using System.Text.RegularExpressions;

namespace Calendario_Estudiantil.Helpers;

/// <summary>
/// Validación de formato de color hexadecimal.
/// </summary>
public static partial class ColorValidation
{
    private const string DefaultColor = "#512BD4";

    /// <summary>
    /// Valida y normaliza un color hex. Retorna el default si es inválido.
    /// </summary>
    public static string Validate(string? color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return DefaultColor;

        var trimmed = color.Trim();

        // Aceptar con o sin #
        if (trimmed.StartsWith('#'))
            trimmed = trimmed.Substring(1);

        // Validar formato RRGGBB (6 chars hex)
        if (trimmed.Length != 6 || !HexRegex().IsMatch(trimmed))
            return DefaultColor;

        return $"#{trimmed.ToUpperInvariant()}";
    }

    [GeneratedRegex(@"^[0-9A-Fa-f]{6}$")]
    private static partial Regex HexRegex();
}
