// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal class PopupAttributes
{
    public string PopupTitle { get; set; }
    public double PopupTop { get; set; }
    public double PopupLeft { get; set; }
    public double PopupHeight { get; set; }
    public double PopupWidth { get; set; }
    public string PopupItemID { get; set; }

    public static List<PopupAttributes> Popups { get; set; }
}
