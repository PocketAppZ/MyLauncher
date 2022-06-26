// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
namespace MyLauncher;

public partial class PopupWindow : Window
{
    #region Properties
    public string PopupTitle { get; set; }
    public string PopupID { get; set; }
    #endregion Properties

    public PopupWindow(Child child)
    {
        InitializeComponent();
        DataContext = this;

        InitPopUp(child);

        PopulateListBox(child);
    }

    #region Init Pop-up
    private void InitPopUp(Child ch)
    {
        PopupID = ch.ItemID;
        PopupTitle = ch.Title;
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
    /// <param name="ch"></param>
    private void PopulateListBox(Child ch)
    {
        IconHelpers.GetIcons(ch.ChildrenOfChild);
        PopupListBox.ItemsSource = ch.ChildrenOfChild;
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
        PopupAttributes pa = PopupAttributes.Popups.Find(x => x.PopupTitle == PopupTitle);

        if (pa != null)
        {
            pa.PopupHeight = Height;
            pa.PopupLeft = Left;
            pa.PopupTop = Top;
            pa.PopupWidth = Width;
            pa.PopupTitle = PopupTitle;
        }
        else
        {
            PopupAttributes newpa = new()
            {
                PopupTitle = PopupTitle,
                PopupHeight = Height,
                PopupWidth = Width,
                PopupTop = Top,
                PopupLeft = Left,
                PopupItemID = PopupID
            };
            PopupAttributes.Popups.Add(newpa);
        }

        JsonSerializerOptions opts = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(PopupAttributes.Popups, opts);

        string jsonfile = JsonHelpers.GetMainListFile().Replace("MyLauncher.json", "Popups.json");
        File.WriteAllText(jsonfile, json);
    }
    #endregion Window Events

    #region Set window size and position
    /// <summary>
    /// Set window size and position. Use saved settings if they exist, otherwise put the window in the center of the screen.
    /// </summary>
    private void PopupPosition()
    {
        PopupAttributes attributes = PopupAttributes.Popups.Find(x => x.PopupTitle == PopupTitle);
        if (attributes != null)
        {
            Left = attributes.PopupLeft;
            Top = attributes.PopupTop;
            Height = attributes.PopupHeight;
            Width = attributes.PopupWidth;
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

    private void ColorZone_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SizeToContent = SizeToContent.Width;
        double width = ActualWidth;
        Thread.Sleep(50);
        SizeToContent = SizeToContent.Manual;
        Width = width + 1;
    }
}
