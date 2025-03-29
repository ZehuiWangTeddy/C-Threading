using System.Globalization;
using TaskManager.Models.Enums;

namespace TaskManager.Converters
{
    internal class StatusToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isInverted = false;

            // Checks if the parameter requires inverted logic (if "Invert" is passed in, then invert)
            if (parameter is string paramString && paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                isInverted = true;
            }

            bool isCompleted = false;

            if (value is string statusString)
            {
                // Convert String to StatusType
                if (Enum.TryParse(typeof(StatusType), statusString, out var statusObject) &&
                    statusObject is StatusType status)
                {
                    isCompleted = (status == StatusType.Completed);
                }
            }
            else if (value is StatusType status)
            {
                // If already is StatusType
                isCompleted = (status == StatusType.Completed);
            }

            // If the state is Completed, it is not visible (unless the logic is reversed)
            bool isVisible = isInverted ? isCompleted : !isCompleted;

            return isVisible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
