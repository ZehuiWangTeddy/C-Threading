using System.Globalization;
using TaskManager.Models.Enums;

namespace TaskManager.Converters;

public class StatusToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string statusString)
        {
            // Convert String to StatusType
            if (Enum.TryParse(typeof(StatusType), statusString, out var statusObject) &&
                statusObject is StatusType status)
                if (status == StatusType.Completed)
                    return Colors.LightCoral; //Todo Replace Color
        }
        else if (value is StatusType status)
        {
            // If already is StatusType
            if (status == StatusType.Completed)
                return Colors.LightCoral;
        }

        // Default Task Item BG Color
        return Colors.DarkGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}