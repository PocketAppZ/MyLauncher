// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public class BoolVisibilityInverter : IValueConverter
{
    // Converter used in settings.xaml
    // Flips true to false & false to true
    // http://www.nullskull.com/faq/1298/enable-or-disable-a-control-with-a-checkbox-using-data-binding.aspx

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool boolValue = (bool)value;
        boolValue = (parameter != null) ? !boolValue : boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
