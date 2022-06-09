// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class PopupWindow : Window
{
    #region Properties
    public string PopupTitle { get; set; }
    public int HostID { get; set; }
    #endregion Properties

    public PopupWindow(string title, int hostID)
    {
        InitializeComponent();
        DataContext = this;

        InitPopUp(title, hostID);

        PopulateListBox(hostID);
    }

    #region Init Pop-up
    private void InitPopUp(string title, int hostID)
    {
        PopupTitle = title;
        HostID = hostID;
        Title = "My Launcher - Pop-Up List";

        // UI size
        double size = MainWindow.UIScale((MySize)UserSettings.Setting.UISize);
        PMain.LayoutTransform = new ScaleTransform(size, size);

        // Font
        SetFontWeight((Weight)UserSettings.Setting.ListBoxFontWeight);

        PopupPosition(hostID);
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 100;
    }
    #endregion Init Pop-up

    #region Load the listbox
    private void PopulateListBox(int hostID)
    {
        BindingList<EntryClass> temp = new();

        foreach (EntryClass entry in EntryClass.Entries)
        {
            if (entry.ChildOfHost == hostID)
            {
                temp.Add(entry);
            }
        }
        MainWindow.GetIcons(temp);
        PopupListBox.ItemsSource = temp;
    }
    #endregion Load the listbox

    #region Mouse Click Events
    private void ListBoxItem_MouseClick(object sender, MouseButtonEventArgs e)
    {
        if (((ListBoxItem)sender).Content is not EntryClass || PopupListBox.SelectedItem == null)
        {
            return;
        }
        if (e.ChangedButton == MouseButton.Right && !UserSettings.Setting.AllowRightButton)
        {
            PopupListBox.SelectedItem = null;
            return;
        }
        EntryClass entry = (EntryClass)PopupListBox.SelectedItem;
        if (entry.EntryType == (int)ListEntryType.Popup)
        {
            _ = MainWindow.OpenPopup(entry, this);
        }
        else
        {
            MainWindow.LaunchApp(entry);
        }
        PopupListBox.SelectedIndex = -1;
    }
    #endregion Mouse Click Events

    #region Button Events
    private void Btn_Click_Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
    #endregion Button Events

    #region Window Events
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        PopupAttributes item = PopupAttributes.Popups.Find(x => x.PopupID == HostID);

        if (item != null)
        {
            item.PopupHeight = Height;
            item.PopupLeft = Left;
            item.PopupTop = Top;
            item.PopupWidth = Width;
            item.PopupTitle = PopupTitle;
        }
        else
        {
            PopupAttributes pa = new()
            {
                PopupID = HostID,
                PopupTitle = PopupTitle,
                PopupHeight = Height,
                PopupWidth = Width,
                PopupTop = Top,
                PopupLeft = Left
            };
            PopupAttributes.Popups.Add(pa);
        }

        JsonSerializerOptions opts = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(PopupAttributes.Popups, opts);

        string jsonfile = MainWindow.GetJsonFile().Replace("MyLauncher.json", "Popups.json");
        File.WriteAllText(jsonfile, json);
    }
    #endregion Window Events

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

    #region Remove minimize and maximize/restore buttons
    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        IntPtr windowHandle = new WindowInteropHelper(this).Handle;
        NativeMethods.DisableMinMaxButtons(windowHandle);
    }
    #endregion Remove minimize and maximize/restore buttons

    #region Set window size and position
    private void PopupPosition(int hostID)
    {
        foreach (PopupAttributes attributes in PopupAttributes.Popups)
        {
            if (attributes != null && attributes.PopupID == hostID)
            {
                Left = attributes.PopupLeft;
                Top = attributes.PopupTop;
                Height = attributes.PopupHeight;
                Width = attributes.PopupWidth;
                return;
            }
        }
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
    }
    #endregion Set window size and position
}
