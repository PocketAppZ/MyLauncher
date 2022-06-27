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

        // UI size
        double size = UIScale((MySize)UserSettings.Setting.UISize);
        MainGrid.LayoutTransform = new ScaleTransform(size, size);

        // Minimize to tray
        if (UserSettings.Setting.MinimizeToTray)
        {
            tbIcon.Visibility = Visibility.Visible;
        }

        // Start minimized
        if (UserSettings.Setting.StartMinimized)
        {
            //Hide();
            WindowState = WindowState.Minimized;
            Debug.WriteLine("Minimized via settings");
        }

        // ListBox event handlers
        RegisterEventHandlers();

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
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

            case nameof(UserSettings.Setting.StartWithWindows):
                if ((bool)newValue)
                {
                    AddStartToRegistry();
                }
                else
                {
                    RemoveStartFromRegistry();
                }
                break;
        }
    }
    #endregion Setting change

    #region Navigation
    /// <summary>
    /// Navigates to the requested dialog or window
    /// </summary>
    /// <param name="selectedIndex"></param>
    private void NavigateToPage(NavPage selectedIndex)
    {
        switch (selectedIndex)
        {
            default:
                NavDrawer.IsLeftDrawerOpen = false;
                break;

            case NavPage.Maintenance:
                NavDrawer.IsLeftDrawerOpen = false;
                ShowWindow<Maintenance>();
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
        Child.Children?.Clear();
        JsonHelpers.ReadJson();
        IconHelpers.GetIcons(Child.Children);
        PopulateMainListBox();
    }
    #endregion Clear and repopulate the listbox

    #region Load the listbox
    /// <summary>
    /// Simply sets the ItemsSource of the MainListBox to the Child.Children ObservableCollection
    /// </summary>
    public void PopulateMainListBox()
    {
        MainListBox.ItemsSource = Child.Children;
    }
    #endregion Load the listbox

    #region Launch app or URI
    /// <summary>
    /// Launch the application, folder or URI
    /// </summary>
    /// <param name="item"></param>
    /// <returns>True if successful, False if not.</returns>
    internal static bool LaunchApp(Child item)
    {
        using Process launch = new();
        try
        {
            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.FilePathOrURI);
            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(item.Arguments);
            launch.StartInfo.UseShellExecute = true;
            _ = launch.Start();
            if (UserSettings.Setting.PlaySound)
            {
                using SoundPlayer soundPlayer = new()
                {
                    Stream = Properties.Resources.Pop
                };
                soundPlayer.Play();
            }
            log.Info($"Opening \"{item.Title}\"");
            if (UserSettings.Setting.MainWindowMinimizeOnLaunch)
            {
                SnackbarMsg.QueueMessage($"{item.Title} launched", 2000);
            }
            return true;
        }
        catch (Win32Exception w) when (w.NativeErrorCode == 2)
        {
            log.Error(w, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error launching \"{item.Title}\"\n\nFile Not Found: {item.FilePathOrURI}",
                "ERROR", ButtonType.Ok).ShowDialog();
            return false;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error launching \"{item.Title}\" {item.FilePathOrURI}\n\n{ex.Message}",
                "ERROR", ButtonType.Ok).ShowDialog();
            return false;
        }
    }
    #endregion Launch app or URI

    #region Open pop up list
    /// <summary>
    /// Open the selected pop-up window
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static bool OpenPopup(Child entry)
    {
        PopupWindow popup = new(entry);

        popup.Show();

        return true;
    }
    #endregion Open pop up list

    #region PopupBox button events

    private void BtnData_Click(object sender, RoutedEventArgs e)
    {
        string dir = AppInfo.AppDirectory;
        TextFileViewer.ViewTextFile(Path.Combine(dir, "MyLauncher.json"));
    }

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
    private static void SetBaseTheme(ThemeType mode)
    {
        //Retrieve the app's existing theme
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        switch (mode)
        {
            case ThemeType.Light:
                theme.SetBaseTheme(Theme.Light);
                break;
            case ThemeType.Dark:
                theme.SetBaseTheme(Theme.Dark);
                break;
            case ThemeType.System:
                if (GetSystemTheme().Equals("light", StringComparison.OrdinalIgnoreCase))
                {
                    theme.SetBaseTheme(Theme.Light);
                }
                else
                {
                    theme.SetBaseTheme(Theme.Dark);
                }
                break;
            default:
                theme.SetBaseTheme(Theme.Light);
                break;
        }

        //Change the app's current theme
        paletteHelper.SetTheme(theme);
    }

    private static string GetSystemTheme()
    {
        BaseTheme? sysTheme = Theme.GetSystemTheme();
        if (sysTheme != null)
        {
            return sysTheme.ToString();
        }
        return string.Empty;
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
    /// Keyboard events
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // With Ctrl
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.L)
            {
                NavigateToPage(NavPage.Maintenance);
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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (UserSettings.Setting.StartMinimized)
        {
            if (UserSettings.Setting.MinimizeToTray)
            {
                Hide();
                tbIcon.ToolTipText = $"My Launcher {AppInfo.TitleVersion}\nClick to Show Main Window";
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }
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
        JsonHelpers.SaveJson();

        // Save settings
        UserSettings.Setting.WindowLeft = Math.Floor(Left);
        UserSettings.Setting.WindowTop = Math.Floor(Top);
        UserSettings.Setting.WindowWidth = Math.Floor(Width);
        UserSettings.Setting.WindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window Events

    #region Tray icon menu events
    internal void TbIconShowMainWindow_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }

    private void TbIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ShowMainWindow();
    }

    private void TbIconSettings_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
        NavigateToPage(NavPage.Settings);
    }

    private void TbIconMaintenance_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
        NavigateToPage(NavPage.Maintenance);
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
    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Topmost = true;
        Focus();
        Thread.Sleep(50);
        Topmost = UserSettings.Setting.KeepOnTop;
    }
    #endregion Show Main window

    #region Add/Remove from registry
    /// <summary>
    /// Add a registry entry to start My Launcher with Windows
    /// </summary>
    private void AddStartToRegistry()
    {
        if (IsLoaded && !RegRun.RegRunEntry("MyLauncher"))
        {
            string result = RegRun.AddRegEntry("MyLauncher", AppInfo.AppPath);
            if (result == "OK")
            {
                log.Info(@"MyLauncher added to HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                _ = new MDCustMsgBox("My Launcher will now start with Windows.",
                                    "My Launcher", ButtonType.Ok).ShowDialog();
            }
            else
            {
                log.Error($"MyLauncher add to startup failed: {result}");
                _ = new MDCustMsgBox("An error has occurred. See the log file",
                                     "My Launcher Error", ButtonType.Ok).ShowDialog();
            }
        }
    }

    /// <summary>
    /// Remove the registry entry
    /// </summary>
    private void RemoveStartFromRegistry()
    {
        if (IsLoaded)
        {
            string result = RegRun.RemoveRegEntry("MyLauncher");
            if (result == "OK")
            {
                log.Info(@"MyLauncher removed from HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                _ = new MDCustMsgBox("My Launcher was removed from Windows startup.",
                                    "My Launcher", ButtonType.Ok).ShowDialog();
            }
            else
            {
                log.Error($"MyLauncher remove from startup failed: {result}");
                _ = new MDCustMsgBox("An error has occurred. See the log file",
                                     "My Launcher Error", ButtonType.Ok).ShowDialog();
            }
        }
    }
    #endregion

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

        _ = new MDCustMsgBox("An error has occurred. See the log file",
            "My Launcher Error", ButtonType.Ok).ShowDialog();
    }
    #endregion Unhandled Exception Handler

    #region ListBox mouse and key events
    /// <summary>
    /// Opens the app or pop-up and optionally closes the pop-up
    /// or minimizes the main window.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBoxMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        ListBoxItem lbi = sender as ListBoxItem;
        ListBox box = WindowHelpers.FindParent<ListBox>(lbi);

        if (lbi.Content is Child entry && box is not null)
        {
            if (!UserSettings.Setting.AllowRightButton && e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            _ = entry.EntryType == ListEntryType.Popup
                ? OpenPopup(entry)
                : LaunchApp(entry);
            box.SelectedItem = null;

            if (box.Name == "PopupListBox" && UserSettings.Setting.PopupCloseAfterLaunch)
            {
                Window window = WindowHelpers.FindParent<Window>(box);
                window.Close();
            }
            if (box.Name == "MainListBox" && UserSettings.Setting.MainWindowMinimizeOnLaunch)
            {
                WindowState = WindowState.Minimized;
            }
        }
    }

    // Todo this needs work
    private void ListBoxKeyDown(object sender, KeyEventArgs e)
    {
        ListBoxItem lbi = sender as ListBoxItem;
        if (lbi.Content is Child entry && e.Key == Key.Enter)
        {
            _ = entry.EntryType == ListEntryType.Popup
                ? OpenPopup(entry)
                : LaunchApp(entry);
            MainListBox.SelectedItem = null;
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

        EventManager.RegisterClassHandler(typeof(ListBoxItem),
            KeyDownEvent,
            new KeyEventHandler(ListBoxKeyDown));
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

    #region Double click ColorZone
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
    #endregion Double click ColorZone
}