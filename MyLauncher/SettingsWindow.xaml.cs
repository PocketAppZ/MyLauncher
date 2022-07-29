// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class SettingsWindow : Window
{
    #region NLog Instance
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    public SettingsWindow()
    {
        InitializeComponent();

        InitSettings();
    }

    #region Settings
    private void InitSettings()
    {
        // Put the version number in the title bar
        Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";

        // Window position
        Top = UserSettings.Setting.SettingsWindowTop;
        Left = UserSettings.Setting.SettingsWindowLeft;
        Height = UserSettings.Setting.SettingsWindowHeight;

        // UI size
        double size = MainWindow.UIScale((MySize)UserSettings.Setting.UISize);
        SettingsGrid.LayoutTransform = new ScaleTransform(size, size);

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
    }
    #endregion Settings

    #region Setting changed
    /// <summary>
    /// Handle relevant settings changes
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        object newValue = prop?.GetValue(sender, null);
        switch (e.PropertyName)
        {
            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = MainWindow.UIScale((MySize)size);
                SettingsGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
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

                case nameof(UserSettings.Setting.MinimizeToTray):
                if (!(bool)newValue)
                {
                    UserSettings.Setting.MinimizeToTrayOnClose = false;
                }
                break;
        }
    }
    #endregion Setting changed

    #region Toggle an increased shadow effect when mouse is over Card
    private void Card_MouseEnter(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth4);
    }

    private void Card_MouseLeave(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth2);
    }
    #endregion Toggle an increased shadow effect when mouse is over Card

    #region Window events
    /// <summary>
    /// Remove the minimize and maximize buttons
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr windowHandle = new WindowInteropHelper(this).Handle;
        NativeMethods.DisableMinMaxButtons(windowHandle);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Save window position
        UserSettings.Setting.SettingsWindowLeft = Math.Floor(Left);
        UserSettings.Setting.SettingsWindowTop = Math.Floor(Top);
        UserSettings.Setting.SettingsWindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window events

    #region Double click ColorZone
    /// <summary>
    /// Double click the ColorZone to set optimal width
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ColorZone_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SizeToContent = SizeToContent.WidthAndHeight;
        double width = ActualWidth;
        Thread.Sleep(50);
        SizeToContent = SizeToContent.Manual;
        Width = width + 1;
    }
    #endregion Double click ColorZone

    #region Add/Remove from registry
    /// <summary>
    /// Add a registry item to start My Launcher with Windows
    /// </summary>
    private void AddStartToRegistry()
    {
        if (IsLoaded && !RegRun.RegRunEntry("MyLauncher"))
        {
            string result = RegRun.AddRegEntry("MyLauncher", AppInfo.AppPath);
            if (result == "OK")
            {
                log.Info(@"MyLauncher added to HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                MDCustMsgBox mbox = new("My Launcher will now start with Windows.",
                                    "My Launcher",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    this,
                                    false);
                _ = mbox.ShowDialog();
            }
            else
            {
                log.Error($"MyLauncher add to startup failed: {result}");
                MDCustMsgBox mbox = new("An error has occurred. See the log file.",
                                    "My Launcher Error",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    this,
                                    true);
                _ = mbox.ShowDialog();
            }
        }
    }

    /// <summary>
    /// Remove the registry item
    /// </summary>
    private void RemoveStartFromRegistry()
    {
        if (IsLoaded)
        {
            string result = RegRun.RemoveRegEntry("MyLauncher");
            if (result == "OK")
            {
                log.Info(@"MyLauncher removed from HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                MDCustMsgBox mbox = new("My Launcher was removed from Windows startup.",
                                    "My Launcher",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    this,
                                    false);
                _ = mbox.ShowDialog();
            }
            else
            {
                log.Error($"MyLauncher remove from startup failed: {result}");
                MDCustMsgBox mbox = new("An error has occurred. See the log file.",
                                    "My Launcher Error",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    this,
                                    true);
                _ = mbox.ShowDialog();
            }
        }
    }
    #endregion
}
