using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Converters;

namespace Calendario_Estudiantil.Converters;

/// <summary>
/// Convierte color Hex (#RRGGBB) a Color de MAUI.
/// </summary>
public class HexToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrEmpty(hex))
        {
            try
            {
                return Color.FromArgb(hex);
            }
            catch { }
        }
        return Colors.Purple;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}