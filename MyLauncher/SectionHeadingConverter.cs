// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal class SectionHeadingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Switch on the parameter
        switch (parameter.ToString().ToLower())
        {
            // font weight
            case "fontweight":
                switch (value)
                {
                    case 0:
                        return "Normal";
                    case 1:
                        return "SemiBold";
                    case 2:
                        return "Bold";
                    default:
                        return "Normal";
                }
            // font style
            case "fontstyle":
                switch (value)
                {
                    case 0:
                        return "Normal";
                    case 1:
                        return "Italic";
                    default:
                        return "Normal";
                }
            // margin - negative values move heading left
            case "offset":
                if (value is int x)
                {
                    int offset = 0;
                    switch (x)
                    {
                        case 0:
                            offset = -15;
                            break;
                        case 1:
                            offset = -10;
                            break;
                        case 2:
                            offset = -5;
                            break;
                        case 3:
                            offset = 0;
                            break;
                        case 4:
                            offset = 5;
                            break;
                        case 5:
                            offset = 10;
                            break;
                        case 6:
                            offset = 15;
                            break;
                    }
                    Thickness thickness = new(offset, 0, 0, 0);
                    return thickness.ToString();
                }
                else
                {
                    Thickness thickness = new(0);
                    return thickness.ToString();
                }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
