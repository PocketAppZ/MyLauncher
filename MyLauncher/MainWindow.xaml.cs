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

        // Font
        SetFontWeight((Weight)UserSettings.Setting.ListBoxFontWeight);

        // Spacing
        SetSpacing((Spacing)UserSettings.Setting.ListBoxSpacing);

        // UI size
        double size = UIScale((MySize)UserSettings.Setting.UISize);
        MainGrid.LayoutTransform = new ScaleTransform(size, size);

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

            case nameof(UserSettings.Setting.ListBoxFontWeight):
                SetFontWeight((Weight)newValue);
                break;

            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = UIScale((MySize)size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting change

    #region Navigation
    private void NavigateToPage(NavPage selectedIndex)
    {
        switch (selectedIndex)
        {
            default:
                NavDrawer.IsLeftDrawerOpen = false;
                break;

            case NavPage.Maintenance:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowManitenanceDialog();
                break;

            case NavPage.Settings:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowSettingsDialog();
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
    public void ResetListBox()
    {
        EntryClass.Entries?.Clear();
        ReadJson();
        GetIcons();
    }
    #endregion Clear and repopulate the listbox

    #region Read the JSON file
    public void ReadJson()
    {
        string jsonfile = GetJsonFile();

        if (!File.Exists(jsonfile))
        {
            CreateNewJson(jsonfile);
        }

        log.Debug($"Reading JSON file: {jsonfile}");
        try
        {
            string json = File.ReadAllText(jsonfile);
            EntryClass.Entries = JsonSerializer.Deserialize<BindingList<EntryClass>>(json);
            listboxEntries.ItemsSource = EntryClass.Entries;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            log.Error(ex, "File or Directory not found {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"File or Directory not found:\n\n{ex.Message}\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error reading file: {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error reading file:\n\n{ex.Message}",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
        }

        if (EntryClass.Entries == null)
        {
            log.Error("File {0} is empty or is invalid", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"File {jsonfile} is empty or is invalid\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
            Environment.Exit(2);
        }

        if (EntryClass.Entries.Count == 1)
        {
            log.Info($"Read {EntryClass.Entries.Count} entry from {jsonfile}");
        }
        else
        {
            log.Info($"Read {EntryClass.Entries.Count} entries from {jsonfile}");
        }
    }
    #endregion Read the JSON file

    #region Get file icons
    public static void GetIcons()
    {
        foreach (EntryClass item in EntryClass.Entries)
        {
            if (!string.IsNullOrEmpty(item.FilePathOrURI))
            {
                if (!string.IsNullOrEmpty(item.IconSource))
                {
                    string image = Path.Combine(AppInfo.AppDirectory, "Icons", item.IconSource);
                    if (File.Exists(image))
                    {
                        log.Debug($"Using image file {item.IconSource} for \"{item.Title}\"");
                        BitmapImage bmi = new();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(image);
                        bmi.EndInit();
                        item.FileIcon = bmi;
                        continue;
                    }
                    log.Debug($"Could not find file {image} to use for \"{item.Title}\"");
                }

                string filePath = item.FilePathOrURI.TrimEnd('\\');
                if (File.Exists(filePath))
                {
                    Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is a file");
                }
                // expand environmental variables for folders
                else if (Directory.Exists(Environment.ExpandEnvironmentVariables(filePath)))
                {
                    Icon temp = Properties.Resources.folder;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is a directory");
                }
                // if complete path wasn't supplied check the path
                else if (filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new(filePath, 2048);
                    bool found = NativeMethods.PathFindOnPath(sb, new string[] { null });
                    if (found)
                    {
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(sb.ToString());
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.FilePathOrURI} was found on the Path");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.FilePathOrURI} was not found on the Path");
                    }
                }
                else if (IsValidUrl(filePath))
                {
                    Icon temp = Properties.Resources.globe;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is valid URL");
                }
                else
                {
                    Icon temp = Properties.Resources.question;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is undetermined");
                }
            }
            else
            {
                Icon temp = Properties.Resources.question;
                item.FileIcon = IconToImageSource(temp);
                log.Debug("Path is empty or null");
            }
        }
    }

    #region Check URL
    private static bool IsValidUrl(string uriName)
    {
        const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        Regex Rgx = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return Rgx.IsMatch(uriName);
    }
    #endregion Check URL

    private static ImageSource IconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            new Int32Rect(0, 0, icon.Width, icon.Height),
            BitmapSizeOptions.FromEmptyOptions());
    }

    #endregion Get file icons

    #region Launch app or URI
    public void ListBoxItem_MouseClick(object sender, MouseButtonEventArgs e)
    {
        if (((ListBoxItem)sender).Content is not EntryClass || listboxEntries.SelectedItem == null)
        {
            return;
        }
        if (e.ChangedButton == MouseButton.Right && !UserSettings.Setting.AllowRightButton)
        {
            listboxEntries.SelectedItem = null;
            return;
        }

        EntryClass entry = (EntryClass)listboxEntries.SelectedItem;
        if (LaunchApp(entry))
        {
            if (UserSettings.Setting.ExitOnOpen)
            {
                Thread.Sleep(250);
                Close();
            }
        }
        listboxEntries.SelectedIndex = -1;
    }

    private static bool LaunchApp(EntryClass item)
    {
        using Process launch = new();
        try
        {
            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.FilePathOrURI);
            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(item.Arguments);
            launch.StartInfo.UseShellExecute = true;
            _ = launch.Start();
            log.Info($"Opening \"{item.Title}\"");
            SnackbarMsg.QueueMessage($"{item.Title} launched");
            if (UserSettings.Setting.PlaySound)
            {
                using SoundPlayer soundPlayer = new()
                {
                    Stream = Properties.Resources.Pop
                };
                soundPlayer.Play();
            }
            return true;
        }
        catch (Win32Exception w) when (w.NativeErrorCode == 2)
        {
            log.Error(w, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error launching \"{item.Title}\"\n\nFile Not Found: {item.FilePathOrURI}", "ERROR", ButtonType.Ok).ShowDialog();
            return false;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error launching \"{item.Title}\" {item.FilePathOrURI}\n\n{ex.Message}", "ERROR", ButtonType.Ok).ShowDialog();
            return false;
        }
    }
    #endregion Launch app or uri

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

    #region Set the row spacing
    /// <summary>
    /// Sets the padding & margin around the items in the listbox
    /// </summary>
    /// <param name="spacing"></param>
    private void SetSpacing(Spacing spacing)
    {
        switch (spacing)
        {
            case Spacing.Compact:
                listboxEntries.ItemContainerStyle = Application.Current.FindResource("ListBoxCompact") as Style;
                break;
            case Spacing.Comfortable:
                listboxEntries.ItemContainerStyle = Application.Current.FindResource("ListBoxComfortable") as Style;
                break;
            case Spacing.Wide:
                listboxEntries.ItemContainerStyle = Application.Current.FindResource("ListBoxSpacious") as Style;
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
                listboxEntries.FontWeight = FontWeights.Thin;
                break;
            case Weight.Regular:
                listboxEntries.FontWeight = FontWeights.Regular;
                break;
            case Weight.SemiBold:
                listboxEntries.FontWeight = FontWeights.SemiBold;
                break;
            case Weight.Bold:
                listboxEntries.FontWeight = FontWeights.Bold;
                break;
            default:
                listboxEntries.FontWeight = FontWeights.Regular;
                break;
        }
    }
    #endregion Set the font weight

    #region UI scale converter
    private static double UIScale(MySize size)
    {
        switch (size)
        {
            case MySize.Smallest:
                return 0.85;
            case MySize.Smaller:
                return 0.92;
            case MySize.Default:
                return 1.0;
            case MySize.Large:
                return 1.05;
            case MySize.Larger:
                return 1.15;
            case MySize.Largest:
                return 1.3;
            default:
                return 1.0;
        }
    }
    #endregion UI scale converter

    #region Keyboard Events
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.L)
            {
                NavigateToPage(NavPage.Maintenance);
            }

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
                if (!DialogHost.IsDialogOpen("MainDialogHost"))
                {
                    DialogHelpers.ShowSettingsDialog();
                }
                else
                {
                    DialogHost.Close("MainDialogHost");
                    DialogHelpers.ShowSettingsDialog();
                }
            }
        }

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
        if (size < 5)
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
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        stopwatch.Stop();
        log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

        // Shut down NLog
        LogManager.Shutdown();

        // Save settings
        UserSettings.Setting.WindowLeft = Math.Floor(Left);
        UserSettings.Setting.WindowTop = Math.Floor(Top);
        UserSettings.Setting.WindowWidth = Math.Floor(Width);
        UserSettings.Setting.WindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window Events

    #region Get the menu JSON file name
    private static string GetJsonFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncher.json");
    }
    #endregion Get the menu JSON file name

    #region Create starter JSON file
    private static void CreateNewJson(string file)
    {
        const string json = /*lang=json,strict*/ "[{\"Title\": \"Calculator\",\"FilePathOrURI\": \"calc.exe\"}]";
        log.Debug($"Creating new JSON file with one entry - {file}");
        File.WriteAllText(file, json);
    }
    #endregion Create starter JSON file

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

    #region Center the list of choices when the window is maximized
    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            MainCard.HorizontalAlignment = HorizontalAlignment.Center;
            MainCard.VerticalAlignment = VerticalAlignment.Center;
        }
        else if (WindowState == WindowState.Normal)
        {
            MainCard.HorizontalAlignment = HorizontalAlignment.Stretch;
            MainCard.VerticalAlignment = VerticalAlignment.Stretch;
        }
        base.OnStateChanged(e);
    }
    #endregion Center the list of choices when the window is maximized

    #region Unhandled Exception Handler
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
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
}