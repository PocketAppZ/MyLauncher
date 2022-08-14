// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher
{
    /// <summary>
    /// Converter that determines whether or not to show the button for run as administrator
    /// </summary>
    internal class RunAsAdminConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is not null and MyListItem listItem
                && listItem.EntryType == ListEntryType.Normal
                && !string.IsNullOrEmpty(values[1].ToString()))
            {
                string text = values[1].ToString();
                if (text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }

            if (values[0] is not null and MyMenuItem menuItem
                && menuItem.ItemType == MenuItemType.MenuItem
                && !string.IsNullOrEmpty(values[1].ToString()))
            {
                string text = values[1].ToString();
                if (text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
