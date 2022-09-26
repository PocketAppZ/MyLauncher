// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class MainWindow : Window
{
    #region NLog Instance
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    #region Stopwatch
    private readonly Stopwatch stopwatch = new();
    #endregion Stopwatch

    public MainWindow()
    {
        InitializeSettings();

        InitializeComponent();

        ReadSettings();

        ResetListBox();

        ResetTrayMenu();
    }

    #region Settings
    /// <summary>
    /// Read and apply settings
    /// </summary>
    private void InitializeSettings()
    {
        stopwatch.Start();

        UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);
    }

    public void ReadSettings()
    {
        // Set NLog configuration
        NLHelpers.NLogConfig(false);

        // Unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Put the version number in the title bar
        Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";

        // Startup message in the temp file
        log.Info($"{AppInfo.AppName} ({AppInfo.AppProduct}) {AppInfo.AppVersion} is starting up");
        log.Info($"{AppInfo.AppCopyright}");
        log.Debug($"{AppInfo.AppName} Build date: {BuildInfo.BuildDateString} UTC");
        log.Debug($"{AppInfo.AppName} Commit ID: {BuildInfo.CommitIDString}");

        // Log the .NET version, app framework and OS platform
        string version = Environment.Version.ToString();
        log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}  ({version})");
        log.Debug($"Framework Version: {AppInfo.Framework}");
        log.Debug($"Operating System: {AppInfo.OsPlatform}");

        // Window position
        Top = UserSettings.Setting.WindowTop;
        Left = UserSettings.Setting.WindowLeft;
        Height = UserSettings.Setting.WindowHeight;
        Width = UserSettings.Setting.WindowWidth;
        Topmost = UserSettings.Setting.KeepOnTop;

        // Light or dark
        SetBaseTheme((ThemeType)UserSettings.Setting.DarkMode);

        // Primary color
        SetPrimaryColor((AccentColor)UserSettings.Setting.PrimaryColor);

        // Secondary color
        SetSecondaryColor((AccentColor)UserSettings.Setting.SecondaryColor);

        // Font
        SetFontWeight((Weight)UserSettings.Setting.ListBoxFontWeight);

        // Spacing
        SetSpacing((Spacing)UserSettings.Setting.ListBoxSpacing);

        // Menu font size
        SetMenuSize((MenuSize)UserSettings.Setting.TrayMenuSize);

        // UI size
        double size = UIScale((MySize)UserSettings.Setting.UISize);
        MainGrid.LayoutTransform = new ScaleTransform(size, size);

        // Minimize to tray
        if (UserSettings.Setting.MinimizeToTray)
        {
            tbIcon.Visibility = Visibility.Visible;
            tbIcon.ToolTipText = $"My Launcher {AppInfo.TitleVersion}";
        }

        // Start minimized
        if (UserSettings.Setting.StartMinimized)
        {
            WindowState = WindowState.Minimized;
            if (UserSettings.Setting.MinimizeToTray)
            {
                Hide();
            }
        }

        // ListBox event handlers
        RegisterEventHandlers();

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
#if DEBUG
        // Suppress harmless binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
    }
    #endregion Settings

    #region Setting change
    private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        object newValue = prop?.GetValue(sender, null);
        log.Debug($"Setting change: \"{e.PropertyName}\" New Value: \"{newValue}\"");
        switch (e.PropertyName)
        {
            case nameof(UserSettings.Setting.KeepOnTop):
                Topmost = (bool)newValue;
                break;

            case nameof(UserSettings.Setting.IncludeDebug):
                NLHelpers.SetLogLevel((bool)newValue);
                break;

            case nameof(UserSettings.Setting.DarkMode):
                SetBaseTheme((ThemeType)newValue);
                break;

            case nameof(UserSettings.Setting.ListBoxSpacing):
                SetSpacing((Spacing)newValue);
                break;

            case nameof(UserSettings.Setting.PrimaryColor):
                SetPrimaryColor((AccentColor)newValue);
                break;

            case nameof(UserSettings.Setting.SecondaryColor):
                SetSecondaryColor((AccentColor)newValue);
                break;

            case nameof(UserSettings.Setting.ListBoxFontWeight):
                SetFontWeight((Weight)newValue);
                break;

            case nameof(UserSettings.Setting.MinimizeToTray):
                tbIcon.Visibility = (bool)newValue ? Visibility.Visible : Visibility.Hidden;
                break;

            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = UIScale((MySize)size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;

            case nameof(UserSettings.Setting.TrayMenuSize):
                SetMenuSize((MenuSize)newValue);
                break;
        }
    }
    #endregion Setting change

    #region Navigation
    /// <summary>
    /// Navigates to the requested dialog or window
    /// </summary>
    /// <param name="selectedIndex"></param>
    public void NavigateToPage(NavPage selectedIndex)
    {
        switch (selectedIndex)
        {
            default:
                NavDrawer.IsLeftDrawerOpen = false;
                break;

            case NavPage.ListMaintenance:
                NavDrawer.IsLeftDrawerOpen = false;
                ShowWindow<Maintenance>();
                break;

            case NavPage.MenuMaintenance:
                NavDrawer.IsLeftDrawerOpen = false;
                ShowWindow<MenuMaint>();
                break;

            case NavPage.Settings:
                NavDrawer.IsLeftDrawerOpen = false;
                ShowWindow<SettingsWindow>();
                break;

            case NavPage.About:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowAboutDialog();
                break;

            case NavPage.Exit:
                App.ExplicitClose = true;
                Application.Current.Shutdown();
                break;
        }
    }

    private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NavigateToPage((NavPage)NavListBox.SelectedIndex);
        NavListBox.SelectedItem = null;
    }
    #endregion Navigation

    #region Clear and repopulate the listbox
    /// <summary>
    /// Clears the ObservableCollection the rereads the JSON file and populates the main ListBox/>
    /// </summary>
    public void ResetListBox()
    {
        MyListItem.Children?.Clear();
        JsonHelpers.ReadJson();
        IconHelpers.GetIcons(MyListItem.Children);
        PopulateMainListBox();
    }
    #endregion Clear and repopulate the listbox

    #region Clear and repopulate the menu
    /// <summary>
    /// Clears the ObservableCollection the rereads the JSON file and populates the tray menu/>
    /// </summary>
    public void ResetTrayMenu()
    {
        MyMenuItem.MLMenuItems?.Clear();
        JsonHelpers.ReadMenuJson();
        PopulateTrayMenu();
    }
    #endregion Clear and repopulate the menu

    #region Load the tray menu
    /// <summary>
    /// Uses a CompositeCollection to add the static menu items to the ItemsSource
    /// </summary>
    public void PopulateTrayMenu()
    {
        // static item at bottom of menu
        MyMenuItem trayMainWindow = new()
        {
            Title = "Show Main Window",
            ItemType = MenuItemType.ShowMainWindow
        };
        // static item at bottom of menu
        MyMenuItem trayExit = new()
        {
            Title = "Exit My Launcher",
            ItemType = MenuItemType.ExitML
        };
        // separates the two items above from the rest of the menu
        MyMenuItem traySeparator = new()
        {
            Title = "-",
            ItemType = MenuItemType.Separator
        };
        trayMenu.ItemsSource = new CompositeCollection()
        {
            new CollectionContainer { Collection = MyMenuItem.MLMenuItems },
            traySeparator,
            trayMainWindow,
            trayExit
        };
    }
    #endregion Load the tray menu

    #region Load the listbox
    /// <summary>
    /// Simply sets the ItemsSource of the MainListBox to the MyListItem.Children ObservableCollection
    /// </summary>
    public void PopulateMainListBox()
    {
        MainListBox.ItemsSource = MyListItem.Children;
    }
    #endregion Load the listbox

    #region Tray menu clicked
    /// <summary>
    /// Click event handler for the tray menu
    /// Launches the app/document/folder/website and handles special cases
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TrayMenu_Click(object sender, RoutedEventArgs e)
    {
        MenuItem mi = e.Source as MenuItem;
        MyMenuItem myMenuItem = mi.Tag as MyMenuItem;
        // Launch app/document/folder/website
        if (myMenuItem.ItemType == MenuItemType.MenuItem)
        {
            MyListItem ch = new()
            {
                FilePathOrURI = myMenuItem.FilePathOrURI,
                Arguments = myMenuItem.Arguments,
                Title = myMenuItem.Title,
                WorkingDir = myMenuItem.WorkingDir,
                RunElevated = myMenuItem.RunElevated,
            };
            _ = LaunchApp(ch);
        }
        // Show Pop-up
        if (myMenuItem.ItemType == MenuItemType.Popup)
        {
            MyListItem pop = PopupHelpers.FindPopup(MyListItem.Children, myMenuItem.PopupID);
            if (pop == null)
            {
                log.Debug($"Couldn't find Pop-up with ID: {myMenuItem.PopupID}");
                return;
            }
            if (pop.EntryType != ListEntryType.Popup)
            {
                log.Debug($"{myMenuItem.PopupID} is not a Pop-up ID");
                return;
            }
            OpenPopup(pop);
        }
        // Exit the app
        else if (myMenuItem.ItemType == MenuItemType.ExitML)
        {
            App.ExplicitClose = true;
            Application.Current.Shutdown();
        }
        // Show the main window
        else if (myMenuItem.ItemType == MenuItemType.ShowMainWindow)
        {
            ShowMainWindow();
        }
        e.Handled = true;
    }
    #endregion Tray menu clicked

    #region Launch app or URI
    /// <summary>
    /// Launch the application, folder or URI
    /// </summary>
    /// <param name="item">MyListItem object to open</param>
    /// <returns>True if successful, False if not.</returns>
    internal static bool LaunchApp(MyListItem item)
    {
        using Process launch = new();
        try
        {
            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.FilePathOrURI);
            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(item.Arguments);
            launch.StartInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables(item.WorkingDir);
            launch.StartInfo.UseShellExecute = true;
            if (item.RunElevated && item.FilePathOrURI.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                launch.StartInfo.Verb = "runas";
            }
            _ = launch.Start();
            if (UserSettings.Setting.PlaySound)
            {
                using SoundPlayer soundPlayer = new()
                {
                    Stream = Properties.Resources.Pop
                };
                soundPlayer.Play();
            }
            if (item.RunElevated)
            {
                log.Info($"Opening \"{item.Title}\" as Administrator");
            }
            else
            {
                log.Info($"Opening \"{item.Title}\"");
            }
            if (Application.Current.MainWindow.Visibility == Visibility.Visible)
            {
                SnackbarMsg.QueueMessage($"{item.Title} launched", 2000);
            }
            return true;
        }
        catch (Win32Exception w) when (w.NativeErrorCode == 2)
        {
            log.Error(w, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error launching \"{item.Title}\"\n\nFile Not Found: {item.FilePathOrURI}",
                                    "My Launcher Error",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    null,
                                    true);
            mbox.Show();
            ShowMainWindow();
            return false;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error launching \"{item.Title}\" \n\n{ex.Message}",
                                    "My Launcher Error",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    null,
                                    true);
            mbox.Show();
            ShowMainWindow();
            return false;
        }
    }
    #endregion Launch app or URI

    #region Open pop up list
    /// <summary>
    /// Open the selected pop-up window
    /// </summary>
    /// <param name="item">MyListItem object to open</param>
    public static void OpenPopup(MyListItem item)
    {
        PopupWindow popup = new(item);

        popup.Show();
    }
    #endregion Open pop up list

    #region PopupBox button events
    private void BtnLog_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(NLHelpers.GetLogfileName());
    }

    private void BtnReadme_Click(object sender, RoutedEventArgs e)
    {
        string dir = AppInfo.AppDirectory;
        TextFileViewer.ViewTextFile(Path.Combine(dir, "ReadMe.txt"));
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        UserSettings.Setting.IncludeDebug = true;
        log.Debug("Current Settings:");
        foreach (KeyValuePair<string, object> item in UserSettings.ListSettings())
        {
            log.Debug($"  {item.Key} = {item.Value}");
        }
        TextFileViewer.ViewTextFile(NLHelpers.GetLogfileName());
    }
    #endregion PopupBox button events

    #region Set light or dark theme
    /// <summary>
    /// Sets the theme
    /// </summary>
    /// <param name="mode">Light, Dark or System</param>
    private void SetBaseTheme(ThemeType mode)
    {
        //Retrieve the app's existing theme
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        if (mode == ThemeType.System)
        {
            mode = GetSystemTheme().Equals("light") ? ThemeType.Light : ThemeType.Dark;
        }

        switch (mode)
        {
            case ThemeType.Light:
                theme.SetBaseTheme(Theme.Light);
                theme.Paper = Colors.WhiteSmoke;
                ThemeAssist.SetTheme(tbIcon.ContextMenu, BaseTheme.Light);
                break;
            case ThemeType.Dark:
                theme.SetBaseTheme(Theme.Dark);
                ThemeAssist.SetTheme(tbIcon.ContextMenu, BaseTheme.Dark);
                break;
            case ThemeType.Darker:
                // Set card and paper background colors a bit darker
                theme.SetBaseTheme(Theme.Dark);
                theme.CardBackground = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF141414");
                theme.Paper = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF202020");
                ThemeAssist.SetTheme(tbIcon.ContextMenu, BaseTheme.Dark);
                break;
            default:
                theme.SetBaseTheme(Theme.Light);
                ThemeAssist.SetTheme(tbIcon.ContextMenu, BaseTheme.Light);
                break;
        }

        //Change the app's current theme
        paletteHelper.SetTheme(theme);
    }

    private static string GetSystemTheme()
    {
        BaseTheme? sysTheme = Theme.GetSystemTheme();
        return (sysTheme != null) ? sysTheme.ToString().ToLower() : string.Empty;
    }
    #endregion Set light or dark theme

    #region Set primary color
    /// <summary>
    /// Sets the MDIX primary accent color
    /// </summary>
    /// <param name="color">One of the 18 color values</param>
    private static void SetPrimaryColor(AccentColor color)
    {
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        PrimaryColor primary;
        switch (color)
        {
            case AccentColor.Red:
                primary = PrimaryColor.Red;
                break;
            case AccentColor.Pink:
                primary = PrimaryColor.Pink;
                break;
            case AccentColor.Purple:
                primary = PrimaryColor.Purple;
                break;
            case AccentColor.DeepPurple:
                primary = PrimaryColor.DeepPurple;
                break;
            case AccentColor.Indigo:
                primary = PrimaryColor.Indigo;
                break;
            case AccentColor.Blue:
                primary = PrimaryColor.Blue;
                break;
            case AccentColor.LightBlue:
                primary = PrimaryColor.LightBlue;
                break;
            case AccentColor.Cyan:
                primary = PrimaryColor.Cyan;
                break;
            case AccentColor.Teal:
                primary = PrimaryColor.Teal;
                break;
            case AccentColor.Green:
                primary = PrimaryColor.Green;
                break;
            case AccentColor.LightGreen:
                primary = PrimaryColor.LightGreen;
                break;
            case AccentColor.Lime:
                primary = PrimaryColor.Lime;
                break;
            case AccentColor.Yellow:
                primary = PrimaryColor.Yellow;
                break;
            case AccentColor.Amber:
                primary = PrimaryColor.Amber;
                break;
            case AccentColor.Orange:
                primary = PrimaryColor.Orange;
                break;
            case AccentColor.DeepOrange:
                primary = PrimaryColor.DeepOrange;
                break;
            case AccentColor.Brown:
                primary = PrimaryColor.Brown;
                break;
            case AccentColor.Grey:
                primary = PrimaryColor.Grey;
                break;
            case AccentColor.BlueGray:
                primary = PrimaryColor.BlueGrey;
                break;
            default:
                primary = PrimaryColor.Blue;
                break;
        }
        System.Windows.Media.Color primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];
        theme.SetPrimaryColor(primaryColor);
        paletteHelper.SetTheme(theme);
    }
    #endregion Set primary color

    #region Set secondary color
    /// <summary>
    /// Sets the MDIX secondary accent color
    /// </summary>
    /// <param name="color">One of the 14 color values</param>
    private static void SetSecondaryColor(AccentColor color)
    {
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        SecondaryColor secondary;
        switch (color)
        {
            case AccentColor.Red:
                secondary = SecondaryColor.Red;
                break;
            case AccentColor.Pink:
                secondary = SecondaryColor.Pink;
                break;
            case AccentColor.Purple:
                secondary = SecondaryColor.Purple;
                break;
            case AccentColor.DeepPurple:
                secondary = SecondaryColor.DeepPurple;
                break;
            case AccentColor.Indigo:
                secondary = SecondaryColor.Indigo;
                break;
            case AccentColor.Blue:
                secondary = SecondaryColor.Blue;
                break;
            case AccentColor.LightBlue:
                secondary = SecondaryColor.LightBlue;
                break;
            case AccentColor.Cyan:
                secondary = SecondaryColor.Cyan;
                break;
            case AccentColor.Teal:
                secondary = SecondaryColor.Teal;
                break;
            case AccentColor.Green:
                secondary = SecondaryColor.Green;
                break;
            case AccentColor.LightGreen:
                secondary = SecondaryColor.LightGreen;
                break;
            case AccentColor.Lime:
                secondary = SecondaryColor.Lime;
                break;
            case AccentColor.Yellow:
                secondary = SecondaryColor.Yellow;
                break;
            case AccentColor.Amber:
                secondary = SecondaryColor.Amber;
                break;
            case AccentColor.Orange:
                secondary = SecondaryColor.Orange;
                break;
            case AccentColor.DeepOrange:
                secondary = SecondaryColor.DeepOrange;
                break;

            default:
                secondary = SecondaryColor.Blue;
                break;
        }
        System.Windows.Media.Color secondaryColor = SwatchHelper.Lookup[(MaterialDesignColor)secondary];
        theme.SetSecondaryColor(secondaryColor);
        paletteHelper.SetTheme(theme);
    }
    #endregion Set secondary color

    #region Set the row spacing
    /// <summary>
    /// Sets the padding & margin around the items in the listbox
    /// </summary>
    /// <param name="spacing"></param>
    private void SetSpacing(Spacing spacing)
    {
        switch (spacing)
        {
            case Spacing.Scrunched:
                MainListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxScrunched") as Style;
                break;
            case Spacing.Compact:
                MainListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxCompact") as Style;
                break;
            case Spacing.Comfortable:
                MainListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxComfortable") as Style;
                break;
            case Spacing.Wide:
                MainListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxSpacious") as Style;
                break;
        }
    }
    #endregion Set the row spacing

    #region Set the font weight
    /// <summary>
    /// Sets the weight of the font in the main window
    /// </summary>
    /// <param name="weight"></param>
    private void SetFontWeight(Weight weight)
    {
        switch (weight)
        {
            case Weight.Thin:
                MainListBox.FontWeight = FontWeights.Thin;
                break;
            case Weight.Regular:
                MainListBox.FontWeight = FontWeights.Regular;
                break;
            case Weight.SemiBold:
                MainListBox.FontWeight = FontWeights.SemiBold;
                break;
            case Weight.Bold:
                MainListBox.FontWeight = FontWeights.Bold;
                break;
            default:
                MainListBox.FontWeight = FontWeights.Regular;
                break;
        }
    }
    #endregion Set the font weight

    #region Set menu size
    /// <summary>
    /// Sets the font size of the tray menu
    /// </summary>
    /// <param name="size">small, medium (default), large or jumbo</param>
    private void SetMenuSize(MenuSize size)
    {
        switch (size)
        {
            case MenuSize.Small:
                trayMenu.FontSize = 11;
                break;
            case MenuSize.Medium:
                trayMenu.FontSize = 13;
                break;
            case MenuSize.Large:
                trayMenu.FontSize = 15;
                break;
            case MenuSize.Jumbo:
                trayMenu.FontSize = 17;
                break;
        }
    }
    #endregion Set menu size

    #region UI scale converter
    /// <summary>
    /// Sets the value for UI scaling
    /// </summary>
    /// <param name="size">One of 7 values</param>
    /// <returns>double used by LayoutTransform</returns>
    internal static double UIScale(MySize size)
    {
        switch (size)
        {
            case MySize.Smallest:
                return 0.7;
            case MySize.Smaller:
                return 0.8;
            case MySize.Small:
                return 0.9;
            case MySize.Default:
                return 1.0;
            case MySize.Large:
                return 1.1;
            case MySize.Larger:
                return 1.2;
            case MySize.Largest:
                return 1.3;
            default:
                return 1.0;
        }
    }
    #endregion UI scale converter

    #region Keyboard Events
    /// <summary>
    /// Keyboard events for window
    /// </summary>
    private void WindowPreview_KeyUp(object sender, KeyEventArgs e)
    {
        // With Ctrl
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.L)
            {
                NavigateToPage(NavPage.ListMaintenance);
            }

            if (e.Key == Key.M)
            {
                NavigateToPage(NavPage.MenuMaintenance);
            }

            if (e.Key == Key.Add)
            {
                EverythingLarger();
            }
            if (e.Key == Key.Subtract)
            {
                EverythingSmaller();
            }
            if (e.Key == Key.OemComma)
            {
                NavigateToPage(NavPage.Settings);
            }
            // Ctrl + numbers 1 - 9
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int k = (int)e.Key - 35;
                if (k <= (MainListBox.Items.Count - 1))
                {
                    var item = MainListBox.Items[k] as MyListItem;
                    if (item.EntryType == ListEntryType.Popup)
                    {
                        OpenPopup(item);
                    }
                    else
                    {
                        LaunchApp(item);
                    }
                    MainListBox.SelectedItem = null;
                }
            }
        }
        // Ctrl and Shift
        if (e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
            if (e.Key == Key.M)
            {
                switch (UserSettings.Setting.DarkMode)
                {
                    case (int)ThemeType.Light:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Dark;
                        break;
                    case (int)ThemeType.Dark:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Darker;
                        break;
                    case (int)ThemeType.Darker:
                        UserSettings.Setting.DarkMode = (int)ThemeType.System;
                        break;
                    case (int)ThemeType.System:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Light;
                        break;
                }
                SnackbarMsg.ClearAndQueueMessage($"Theme set to {(ThemeType)UserSettings.Setting.DarkMode}");
            }
            if (e.Key == Key.P)
            {
                if (UserSettings.Setting.PrimaryColor >= (int)AccentColor.BlueGray)
                {
                    UserSettings.Setting.PrimaryColor = 0;
                }
                else
                {
                    UserSettings.Setting.PrimaryColor++;
                }
                SnackbarMsg.ClearAndQueueMessage($"Accent color set to {(AccentColor)UserSettings.Setting.PrimaryColor}");
            }
            if (e.Key == Key.S)
            {
                if (UserSettings.Setting.SecondaryColor >= (int)AccentColor.DeepOrange)
                {
                    UserSettings.Setting.SecondaryColor = 0;
                }
                else
                {
                    UserSettings.Setting.SecondaryColor++;
                }
                SnackbarMsg.ClearAndQueueMessage($"Accent color set to {(AccentColor)UserSettings.Setting.SecondaryColor}");
            }
        }

        // No Ctrl or Shift
        if (e.Key == Key.F1)
        {
            if (!DialogHost.IsDialogOpen("MainDialogHost"))
            {
                DialogHelpers.ShowAboutDialog();
            }
            else
            {
                DialogHost.Close("MainDialogHost");
                DialogHelpers.ShowAboutDialog();
            }
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Catching Alt+F4 apparently needs to be done on the key down event
        if (e.Key == Key.System && e.SystemKey == Key.F4)
        {
            App.ExplicitClose = true;
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Keyboard events for ListBox
    /// </summary>
    private void ListBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (MainListBox.SelectedItem != null && e.Key == Key.Enter && MainListBox.SelectedItem is MyListItem item)
        {
            if (item.EntryType == ListEntryType.Popup)
            {
                OpenPopup(item);
            }
            else
            {
                LaunchApp(item);
            }
            MainListBox.SelectedItem = null;
        }
        if (MainListBox.SelectedItem != null && e.Key == Key.Escape)
        {
            MainListBox.SelectedItem = null;
        }
    }
    #endregion Keyboard Events

    #region Smaller/Larger
    /// <summary>
    /// Scale the UI according to user preference
    /// </summary>
    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
            return;

        if (e.Delta > 0)
        {
            EverythingLarger();
        }
        else if (e.Delta < 0)
        {
            EverythingSmaller();
        }
    }

    public void EverythingSmaller()
    {
        int size = UserSettings.Setting.UISize;
        if (size > 0)
        {
            size--;
            UserSettings.Setting.UISize = size;
            double newSize = UIScale((MySize)size);
            MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            SnackbarMsg.ClearAndQueueMessage($"Size set to {(MySize)UserSettings.Setting.UISize}");
        }
    }

    public void EverythingLarger()
    {
        int size = UserSettings.Setting.UISize;
        if (size < (int)MySize.Largest)
        {
            size++;
            UserSettings.Setting.UISize = size;
            double newSize = UIScale((MySize)size);
            MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            SnackbarMsg.ClearAndQueueMessage($"Size set to {(MySize)UserSettings.Setting.UISize}");
        }
    }
    #endregion Smaller/Larger

    #region Window Events
    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            if (UserSettings.Setting.MinimizeToTray)
            {
                Hide();
            }
        }
        else if (WindowState == WindowState.Maximized)
        {
            MainCard.HorizontalAlignment = HorizontalAlignment.Center;
            MainCard.VerticalAlignment = VerticalAlignment.Center;
        }
        else if (WindowState == WindowState.Normal)
        {
            MainCard.HorizontalAlignment = HorizontalAlignment.Stretch;
            MainCard.VerticalAlignment = VerticalAlignment.Stretch;
        }
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        _ = MainListBox.Focus();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        stopwatch.Stop();
        log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

        // Shut down NLog
        LogManager.Shutdown();

        // Dispose of the tray icon
        tbIcon.Dispose();

        // Save the JSON on file
        JsonHelpers.SaveMainJson();

        // Save settings
        UserSettings.Setting.WindowLeft = Math.Floor(Left);
        UserSettings.Setting.WindowTop = Math.Floor(Top);
        UserSettings.Setting.WindowWidth = Math.Floor(Width);
        UserSettings.Setting.WindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Skip this if Windows is shutting down
        if (!App.WindowsLogoffOrShutdown)
        {
            // Intercept close from the X in the title bar or from the task bar or Alt+F4
            // in case user didn't really mean to exit.
            if (!App.ExplicitClose && UserSettings.Setting.MinimizeToTray)
            {
                // Is option set to minimize to tray?
                if (UserSettings.Setting.MinimizeToTrayOnClose)
                {
                    Hide();
                    e.Cancel = true;
                }
                else
                {
                    // Ask user
                    SystemSounds.Hand.Play();
                    MDCustMsgBox mbox = new("Do you want to exit My Launcher?",
                        "Exit My Launcher",
                        ButtonType.YesNo,
                        true,
                        true,
                        this,
                        false);
                    _ = mbox.ShowDialog();
                    if (MDCustMsgBox.CustResult == CustResultType.No)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        //clean up notify icon (would otherwise stay after application finishes)
                        tbIcon.Dispose();
                        base.OnClosing(e);
                    }
                }
            }
            // Exit selected from menu so don't ask intention
            else
            {
                tbIcon.Dispose();
                base.OnClosing(e);
            }
        }
        else
        {
            log.Info("My Launcher is closing due to user log off or Windows shutdown");
        }
    }

#pragma warning disable RCS1163 // Unused parameter.
    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
#pragma warning restore RCS1163 // Unused parameter.
    {
        // e.NewValue - True is visible - False is not
        if ((bool)e.NewValue)
        {
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;
            Height = UserSettings.Setting.WindowHeight;
            Width = UserSettings.Setting.WindowWidth;
            Topmost = UserSettings.Setting.KeepOnTop;
        }
        else
        {
            UserSettings.Setting.WindowLeft = Math.Floor(Left);
            UserSettings.Setting.WindowTop = Math.Floor(Top);
            UserSettings.Setting.WindowWidth = Math.Floor(Width);
            UserSettings.Setting.WindowHeight = Math.Floor(Height);
        }
    }
    #endregion Window Events

    #region Tray icon menu events
    internal void TbIconShowMainWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }
    #endregion Tray icon menu events

    #region Mouse enter/leave shadow effect
    private void Card_MouseEnter(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth3);
    }

    private void Card_MouseLeave(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth2);
    }
    #endregion Mouse enter/leave shadow effect

    #region Exit button event
    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
    #endregion Exit button event

    #region Show Main window
    /// <summary>
    /// Show the main window and set it's state to normal
    /// </summary>
    private static void ShowMainWindow()
    {
        Application.Current.MainWindow.Show();
        Application.Current.MainWindow.Visibility = Visibility.Visible;
        Application.Current.MainWindow.WindowState = WindowState.Normal;
        _ = Application.Current.MainWindow.Activate();
    }
    #endregion Show Main window

    #region Unhandled Exception Handler
    /// <summary>
    /// Handles any exceptions that weren't caught by a try-catch statement
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        log.Error("Unhandled Exception");
        Exception e = (Exception)args.ExceptionObject;
        log.Error(e.Message);
        if (e.InnerException != null)
        {
            log.Error(e.InnerException.ToString());
        }
        log.Error(e.StackTrace);
        MDCustMsgBox mbox = new("An error has occurred. See the log file.",
                            "My Launcher Error",
                            ButtonType.Ok,
                            true,
                            true,
                            null,
                            true);
        _ = mbox.ShowDialog();
        ShowMainWindow();
    }
    #endregion Unhandled Exception Handler

    #region ListBox mouse events
    /// <summary>
    /// Opens the app or pop-up and
    /// optionally closes the pop-up or minimizes the main window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBoxMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        ListBoxItem lbi = sender as ListBoxItem;
        ListBox box = WindowHelpers.FindParent<ListBox>(lbi);

        if (lbi.Content is MyListItem entry && box is not null)
        {
            if (!UserSettings.Setting.AllowRightButton && e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (entry.EntryType == ListEntryType.Popup)
            {
                OpenPopup(entry);
            }
            else
            {
                LaunchApp(entry);
            }
            box.SelectedItem = null;

            if (box.Name == "PopupListBox" && UserSettings.Setting.PopupCloseAfterLaunch)
            {
                Window window = WindowHelpers.FindParent<Window>(box);
                window.Close();
            }
            else if (box.Name == "MainListBox" && UserSettings.Setting.MainWindowMinimizeOnLaunch && !UserSettings.Setting.MinimizeToTray)
            {
                WindowState = WindowState.Minimized;
            }
            else if (box.Name == "MainListBox" && UserSettings.Setting.MainWindowMinimizeOnLaunch && UserSettings.Setting.MinimizeToTray)
            {
                Hide();
            }
        }
    }

    /// <summary>
    /// These event handlers affect the entire application!
    /// </summary>
    private void RegisterEventHandlers()
    {
        EventManager.RegisterClassHandler(typeof(ListBoxItem),
            MouseUpEvent,
            new MouseButtonEventHandler(ListBoxMouseButtonUp));
    }
    #endregion ListBox mouse and key events

    #region Show a single instance of a window
    /// <summary>
    /// Show a single instance of a window. If window is already shown, set its state to Normal and Activate it.
    /// </summary>
    /// <typeparam name="T">window type</typeparam>
    /// https://stackoverflow.com/a/64353092/15237757
    public static void ShowWindow<T>() where T : Window, new()
    {
        T existingWindow = Application.Current.Windows.OfType<T>().SingleOrDefault();

        if (existingWindow == null)
        {
            new T().Show();
            return;
        }

        existingWindow.WindowState = WindowState.Normal;
        existingWindow.Activate();
    }
    #endregion Show a single instance of a window

    #region ColorZone mouse events
    /// <summary>
    /// Double click the ColorZone to set optimal width
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ColorZone_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SizeToContent = SizeToContent.Width;
        double width = ActualWidth;
        Thread.Sleep(50);
        SizeToContent = SizeToContent.Manual;
        Width = width + 1;
    }

    /// <summary>
    /// Allow mouse to drag window with left button down in ColorZone
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The instance containing the event data.</param>
    private void ColorZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
    #endregion ColorZone mouse events
}