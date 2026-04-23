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

/// <summary>
/// Convierte booleano a color para celdas (true = tiene clase).
/// </summary>
public class BoolToClassColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool hasClass && hasClass)
        {
            return Color.FromArgb("#512BD4"); // Color por defecto de materia
        }
        return Color.FromArgb("#FAFAFA"); // Celdas vacías
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convierte booleano a color para headers de día (true = es hoy).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isToday && isToday)
        {
            return Color.FromArgb("#E3F2FD"); // Color destacado para hoy
        }
        return Color.FromArgb("#EEEEEE"); // Color normal
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}