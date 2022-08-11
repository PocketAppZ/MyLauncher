// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher
{
    internal class RunAsAdminConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MyListItem myListItem)
            {
                MyListItem item = myListItem;
                return item?.EntryType == ListEntryType.Normal
                       && item.FilePathOrURI.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            if (value is MyMenuItem myMenuItem)
            {
                MyMenuItem item = myMenuItem;
                return item?.ItemType == MenuItemType.MenuItem
                       && item.FilePathOrURI.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
