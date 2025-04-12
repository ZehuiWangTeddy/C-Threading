using System.Globalization;

namespace TaskManager.Converters;

public class ComparisonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.Equals(parameter);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return parameter;
        return Binding.DoNothing;
    }
}