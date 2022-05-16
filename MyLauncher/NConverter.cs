// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Converts null to "untitled"
/// </summary>
public class NConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "untitled";
        }
        return string.IsNullOrEmpty(value.ToString()) ? "untitled" : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return "?";
    }
}
