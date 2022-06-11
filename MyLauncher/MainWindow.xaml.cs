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

        ReadPopupsJson();
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
        GetIcons(EntryClass.Entries);
        PopulateMainListBox();
    }
    #endregion Clear and repopulate the listbox

    #region Load the listbox
    public void PopulateMainListBox()
    {
        BindingList<EntryClass> temp = new();
        foreach (EntryClass entry in EntryClass.Entries)
        {
            if (entry.ChildOfHost == 0)
            {
                temp.Add(entry);
            }
        }
        listboxEntries.ItemsSource = temp;
    }
    #endregion Load the listbox

    #region Read Pop-ups file
    public static void ReadPopupsJson()
    {
        string jsonfile = GetJsonFile().Replace("MyLauncher.json", "Popups.json");

        if (!File.Exists(jsonfile))
        {
            File.WriteAllText(jsonfile, "[]");
        }
        string json = File.ReadAllText(jsonfile);
        PopupAttributes.Popups = JsonSerializer.Deserialize<List<PopupAttributes>>(json);
        log.Info($"Read {PopupAttributes.Popups.Count} entries from {jsonfile}");
    }
    #endregion Read Pop-ups file

    #region Read the JSON file
    public static void ReadJson()
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
    public static void GetIcons(BindingList<EntryClass> ec)
    {
        foreach (EntryClass item in ec)
        {
            if (!string.IsNullOrEmpty(item.FilePathOrURI) || item.EntryType == (int)ListEntryType.Popup)
            {
                if (!string.IsNullOrEmpty(item.IconSource))
                {
                    string image = Path.Combine(AppInfo.AppDirectory, "Icons", item.IconSource);
                    if (File.Exists(image))
                    {
                        log.Debug($"Using image file {item.IconSource} for \"{item.Title}\".");
                        BitmapImage bmi = new();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(image);
                        bmi.EndInit();
                        item.FileIcon = bmi;
                        //log.Debug($"Image size is {bmi.PixelHeight} x {bmi.PixelWidth}");
                        continue;
                    }
                    log.Debug($"Could not find file {image} to use for \"{item.Title}\".");
                }

                if (item.EntryType == (int)ListEntryType.Popup)
                {
                    string image = Path.Combine(AppInfo.AppDirectory, "Icons", "UpArrow.png");
                    if (File.Exists(image))
                    {
                        log.Debug($"Using image file UpArrow.png for \"{item.Title}\".");
                        BitmapImage bmi = new();
                        bmi.BeginInit();
                        bmi.UriSource = new Uri(image);
                        bmi.EndInit();
                        item.FileIcon = bmi;
                        continue;
                    }
                    log.Debug($"Could not find file {image} to use for \"{item.Title}\".");
                }

                string filePath = item.FilePathOrURI.TrimEnd('\\');
                if (File.Exists(filePath))
                {
                    if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        string shortcut = ((IWshShortcut)new WshShell().CreateShortcut(filePath)).TargetPath;
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(shortcut);
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"Using extracted associated icon from shortcut to {item.FilePathOrURI}.");
                    }
                    else
                    {
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"Using extracted associated icon for {item.FilePathOrURI}.");
                    }
                }
                // expand environmental variables for folders
                else if (Directory.Exists(Environment.ExpandEnvironmentVariables(filePath)))
                {
                    Icon temp = Properties.Resources.folder;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"Using folder icon for {item.FilePathOrURI}.");
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
                        log.Debug($"Using extracted associated icon for {item.FilePathOrURI}.");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question;
                        item.FileIcon = IconToImageSource(temp);
                        log.Warn($"Icon for {item.FilePathOrURI} could not be located.");
                    }
                }
                // maybe it's a url
                else if (IsValidUrl(filePath))
                {
                    Icon temp = Properties.Resources.globe;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"Using globe icon for {item.FilePathOrURI}. It appears to be a valid URL.");
                }
                else
                {
                    Icon temp = Properties.Resources.question;
                    item.FileIcon = IconToImageSource(temp);
                    log.Warn($"Icon for {item.FilePathOrURI} could not be located.");
                }
            }
            // this shouldn't happen
            else
            {
                Icon temp = Properties.Resources.question;
                item.FileIcon = IconToImageSource(temp);
                log.Warn("Path is empty or null. Icon cannot be assigned.");
            }
        }
    }

    private static ImageSource IconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            new Int32Rect(0, 0, icon.Width, icon.Height),
            BitmapSizeOptions.FromEmptyOptions());
    }
    #endregion Get file icons

    #region Check URL
    private static bool IsValidUrl(string uriName)
    {
        const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        Regex Rgx = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return Rgx.IsMatch(uriName);
    }
    #endregion Check URL

    #region Launch app or URI
    internal static bool LaunchApp(EntryClass item)
    {
        using Process launch = new();
        try
        {
            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.FilePathOrURI);
            launch.StartInfo.Arguments = Environment.ExpandEnvironmentVariables(item.Arguments);
            launch.StartInfo.UseShellExecute = true;
            _ = launch.Start();
            log.Info($"Opening \"{item.Title}\"");
            SnackbarMsg.QueueMessage($"{item.Title} launched", 2000);
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

    #region Open pop up list
    public static bool OpenPopup(EntryClass entry)
    {
        if (entry.EntryType != (int)ListEntryType.Popup)
        {
            return false;
        }

        PopupWindow popup = new(entry.Title, entry.HostID);

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

    #region Set secondary color
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
                listboxEntries.ItemContainerStyle = Application.Current.FindResource("ListBoxScrunched") as Style;
                break;
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
    internal static double UIScale(MySize size)
    {
        switch (size)
        {
            case MySize.Smallest:
                return 0.60;
            case MySize.Smaller:
                return 0.70;
            case MySize.Small:
                return 0.80;
            case MySize.Default:
                return 0.9;
            case MySize.Large:
                return 1.0;
            case MySize.Larger:
                return 1.15;
            case MySize.Largest:
                return 1.3;
            default:
                return 0.9;
        }
    }
    #endregion UI scale converter

    #region Keyboard Events
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

    #region Get the menu JSON file name
    public static string GetJsonFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncher.json");
    }
    #endregion Get the menu JSON file name

    #region Create starter JSON file
    private static void CreateNewJson(string file)
    {
        StringBuilder sb = new();
        _ = sb.AppendLine("[")
            .AppendLine("  {")
            .AppendLine("    \"Arguments\": \"\",")
            .AppendLine("    \"ChildOfHost\": 0,")
            .AppendLine("    \"EntryType\": 0,")
            .AppendLine("    \"FilePathOrURI\": \"calc.exe\",")
            .AppendLine("    \"HostID\": -1,")
            .AppendLine("    \"IconSource\": \"\",")
            .AppendLine("    \"Title\": \"Calculator\" ")
            .AppendLine("  }")
            .AppendLine("]");
        File.WriteAllText(file, sb.ToString());
        log.Debug($"Creating new JSON file with one entry - {file}");
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

    #region ListBox mouse and key events
    private void ListBoxMouseButtonUp(object sender, MouseButtonEventArgs e)
    {
        ListBoxItem lbi = sender as ListBoxItem;
        ListBox box = WindowHelpers.FindParent<ListBox>(lbi);

        if (box is null or not ListBox || box.Name == "MaintListBox")
        {
            return;
        }

        if (lbi.Content is EntryClass entry)
        {
            if (!UserSettings.Setting.AllowRightButton && e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if (entry.EntryType == (int)ListEntryType.Popup)
            {
                _ = OpenPopup(entry);
            }
            else
            {
                _ = LaunchApp(entry);
            }
            listboxEntries.SelectedItem = null;
        }
    }

    private void ListBoxKeyDown(object sender, KeyEventArgs e)
    {
        ListBoxItem lbi = sender as ListBoxItem;
        if (lbi.Content is EntryClass entry && e.Key == Key.Enter)
        {
            if (entry.EntryType == (int)ListEntryType.Popup)
            {
                _ = OpenPopup(entry);
            }
            else
            {
                _ = LaunchApp(entry);
            }
            listboxEntries.SelectedItem = null;
        }
    }

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
}