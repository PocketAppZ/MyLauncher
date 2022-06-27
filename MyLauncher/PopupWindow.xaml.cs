// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class PopupWindow : Window
{
    #region Properties
    public string PopupTitle { get; set; }
    public string PopupID { get; set; }
    public Child ThisPopup { get; set; }
    #endregion Properties

    public PopupWindow(Child child)
    {
        InitializeComponent();
        DataContext = this;

        InitPopUp(child);

        PopulateListBox(child);
    }

    #region Init Pop-up
    private void InitPopUp(Child child)
    {
        ThisPopup = child;
        PopupID = ThisPopup.ItemID;
        PopupTitle = ThisPopup.Title;
        Title = "My Launcher - Pop-Up List";

        // UI size
        double size = MainWindow.UIScale((MySize)UserSettings.Setting.UISize);
        PopMain.LayoutTransform = new ScaleTransform(size, size);

        // Font
        SetFontWeight((Weight)UserSettings.Setting.ListBoxFontWeight);

        // Spacing
        SetSpacing((Spacing)UserSettings.Setting.ListBoxSpacing);

        // Position & Size
        PopupPosition();
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 100;

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
    }
    #endregion Init Pop-up

    #region Setting changed
    private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        object newValue = prop?.GetValue(sender, null);
        switch (e.PropertyName)
        {
            case nameof(UserSettings.Setting.KeepOnTop):
                Topmost = (bool)newValue;
                break;

            case nameof(UserSettings.Setting.ListBoxFontWeight):
                SetFontWeight((Weight)newValue);
                break;

            case nameof(UserSettings.Setting.ListBoxSpacing):
                SetSpacing((Spacing)newValue);
                break;

            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = MainWindow.UIScale((MySize)size);
                PopMain.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting changed

    #region Load the listbox
    /// <summary>
    /// Populate the ListBox with the Child items for this pop-up.
    /// </summary>
    /// <param name="child"></param>
    private void PopulateListBox(Child child)
    {
        IconHelpers.GetIcons(child.ChildrenOfChild);
        PopupListBox.ItemsSource = child.ChildrenOfChild;
    }
    #endregion Load the listbox

    #region Window Events
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

    /// <summary>
    /// Save the window position on closing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        SavePopupPosition(ThisPopup);
    }
    #endregion Window Events

    #region Save pop-up size and position

    /// <summary>
    /// Save the pop-up window size and position
    /// </summary>
    /// <param name="child"></param>
    public void SavePopupPosition(Child child)
    {
        child.PopupHeight = Height;
        child.PopupLeft = Left;
        child.PopupTop = Top;
        child.PopupWidth = Width;
    }
    #endregion Save pop-up size and position

    #region Set window size and position
    /// <summary>
    /// Set window size and position. Use saved settings if they exist, otherwise put the window in the center of the screen.
    /// </summary>
    private void PopupPosition()
    {
        if (ThisPopup.PopupWidth != 0)
        {
            Left = ThisPopup.PopupLeft;
            Top = ThisPopup.PopupTop;
            Height = ThisPopup.PopupHeight;
            Width = ThisPopup.PopupWidth;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
        }
    }
    #endregion Set window size and position

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
                PopupListBox.FontWeight = FontWeights.Thin;
                break;
            case Weight.Regular:
                PopupListBox.FontWeight = FontWeights.Regular;
                break;
            case Weight.SemiBold:
                PopupListBox.FontWeight = FontWeights.SemiBold;
                break;
            case Weight.Bold:
                PopupListBox.FontWeight = FontWeights.Bold;
                break;
            default:
                PopupListBox.FontWeight = FontWeights.Regular;
                break;
        }
    }
    #endregion Set the font weight

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
                PopupListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxScrunched") as Style;
                break;
            case Spacing.Compact:
                PopupListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxCompact") as Style;
                break;
            case Spacing.Comfortable:
                PopupListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxComfortable") as Style;
                break;
            case Spacing.Wide:
                PopupListBox.ItemContainerStyle = Application.Current.FindResource("ListBoxSpacious") as Style;
                break;
        }
    }
    #endregion Set the row spacing

    #region Double click ColorZone
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
