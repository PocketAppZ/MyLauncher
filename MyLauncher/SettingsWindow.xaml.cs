// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
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

    private void ColorZone_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SizeToContent = SizeToContent.Width;
        Thread.Sleep(50);
        SizeToContent = SizeToContent.Manual;
    }
}
