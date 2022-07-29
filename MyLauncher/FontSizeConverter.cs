// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal class FontSizeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Current value - from the context menu
        double curValue = System.Convert.ToDouble(values[0]);
        // Delta value - from settings
        double delta = System.Convert.ToDouble(values[1]);
        return curValue + delta;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
