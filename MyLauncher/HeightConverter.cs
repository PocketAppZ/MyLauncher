// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Convert the value from settings to the height of menu items in tray menu
/// </summary>
internal class HeightConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case 0:
                return 20;
            case 1:
                return 25;
            case 2:
                return 30;
            case 3:
                return 40;
            default:
                return 30;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
