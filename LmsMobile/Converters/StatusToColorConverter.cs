using System.Globalization;

namespace LmsMobile.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "Approved" => Colors.SeaGreen,
            "Rejected" => Colors.IndianRed,
            "Inactive" => Colors.Gray,
            "Active" => Colors.SeaGreen,
            _ => Colors.Orange
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
