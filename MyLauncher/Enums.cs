// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;
/// <summary>
/// Accent color enumeration applies to both Primary and Secondary colors
/// </summary>
internal enum AccentColor
{
    Red = 0,
    Pink = 1,
    Purple = 2,
    DeepPurple = 3,
    Indigo = 4,
    Blue = 5,
    LightBlue = 6,
    Cyan = 7,
    Teal = 8,
    Green = 9,
    LightGreen = 10,
    Lime = 11,
    Yellow = 12,
    Amber = 13,
    Orange = 14,
    DeepOrange = 15,
    Brown = 16,
    Grey = 17,
    BlueGray = 18
}

/// <summary>
/// Main list entry type
/// </summary>
public enum ListEntryType
{
    Normal = 0,
    Popup = 1
}

/// <summary>
/// Size of the UI windows
/// </summary>
internal enum MySize
{
    Smallest = 0,
    Smaller = 1,
    Small = 2,
    Default = 3,
    Large = 4,
    Larger = 5,
    Largest = 6
}

/// <summary>
/// Navigation in the app.
/// </summary>
public enum NavPage
{
    Daily = 0,
    ListMaintenance = 1,
    MenuMaintenance = 2,
    Settings = 3,
    About = 4,
    Exit = 5
}

/// <summary>
/// Spacing of the ListBoxes in the main and pop-up windows
/// </summary>
internal enum Spacing
{
    Scrunched = 0,
    Compact = 1,
    Comfortable = 2,
    Wide = 3
}

/// <summary>
/// Theme type
/// </summary>
internal enum ThemeType
{
    Light = 0,
    Dark = 1,
    System = 2
}

/// <summary>
///Font weight for ListBox items
/// </summary>
internal enum Weight
{
    Thin = 0,
    Regular = 1,
    SemiBold = 2,
    Bold = 3
}

/// <summary>
/// Type of the menu entries. ExitML is used to exit the app. ShowMainWindow will show the main window.
/// </summary>
public enum MenuItemType
{
    SubMenu = 0,
    MenuItem = 1,
    ExitML = 2,
    Separator = 3,
    ShowMainWindow = 4,
    SectionHead = 5
}

/// <summary>
/// Size of font in the tray menu
/// </summary>
public enum MenuSize
{
    Small = 0,
    Medium = 1,
    Large = 2,
    Jumbo = 3
}