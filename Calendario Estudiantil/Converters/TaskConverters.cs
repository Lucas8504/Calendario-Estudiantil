using System.Globalization;
using Calendario_Estudiantil.Models;
using Microsoft.Maui.Converters;

namespace Calendario_Estudiantil.Converters;

public class TaskStatusToEmojiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ItemStatus status)
        {
            return status switch
            {
                ItemStatus.Completed => "✅",
                ItemStatus.Expired => "❌",
                _ => "⏳"
            };
        }
        return "⏳";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class SubjectIdToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int id)
            return id > 0;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
