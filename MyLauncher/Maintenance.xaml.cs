// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class Maintenance : Window
{
    #region NLog
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog

    #region Properties
    /// <summary>
    /// Indicates if the list of entries has changed
    /// </summary>
    public static bool EntriesChanged { get; set; }
    #endregion Properties

    public Maintenance()
    {
        InitializeComponent();

        InitSettings();

        LoadTreeView();

        DataContext = Child.Children;
    }

    #region Settings
    private void InitSettings()
    {
        // Put the version number in the title bar
        Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";

        // If this is the first run place the window in the center of the screen
        if (UserSettings.Setting.MaintFistRun)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UserSettings.Setting.MaintFistRun = false;
        }
        else
        {
            // Window position
            Top = UserSettings.Setting.MaintWindowTop;
            Left = UserSettings.Setting.MaintWindowLeft;
        }
        Height = UserSettings.Setting.MaintWindowHeight;
        Width = UserSettings.Setting.MaintWindowWidth;

        // UI size
        double size = MainWindow.UIScale((MySize)UserSettings.Setting.UISize);
        MaintGrid.LayoutTransform = new ScaleTransform(size, size);

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;

        //Child.Entries.ListChanged += Entries_ListChanged;
        Child.Children.CollectionChanged += Children_CollectionChanged;
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
                MaintGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting changed

    #region Window events
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Select the first item in the TreeView
        _ = TvMaint.Focus();
        if (TvMaint.Items.Count > 0)
        {
            Child first = Child.Children.FirstOrDefault();
            if (TvMaint.ItemContainerGenerator.ContainerFromItem(first) is TreeViewItem tvi)
            {
                tvi.IsSelected = true;
            }
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Clear any remaining messages
        SnackBarMaint.MessageQueue.Clear();

        // Save window position
        UserSettings.Setting.MaintWindowLeft = Math.Floor(Left);
        UserSettings.Setting.MaintWindowTop = Math.Floor(Top);
        UserSettings.Setting.MaintWindowWidth = Math.Floor(Width);
        UserSettings.Setting.MaintWindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window events

    #region Load the TreeView
    /// <summary>
    /// Loads the TreeView with the contents of the Observable Collection
    /// </summary>
    private void LoadTreeView()
    {
        if (Child.Children is not null)
        {
            TvMaint.ItemsSource = Child.Children;
        }
    }
    #endregion Load the TreeView

    #region Check for "untitled" entries in the list box
    /// <summary>
    /// Checks the observable collection for any Title equal to "untitled"
    /// </summary>
    /// <returns>True if not found. False if found.</returns>
    private bool CheckForUntitled()
    {
        // Loop through the list backwards checking for "untitled" entries
        if (Child.Children.Any(x => string.Equals(x.Title, "untitled", StringComparison.OrdinalIgnoreCase)))
        {
            log.Error("New item prohibited, \"untitled\" entry in list");
            MDCustMsgBox mbox = new("Please update or delete the \"untitled\" entry before adding another new entry.",
                                    "ERROR",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    this,
                                    true);
            mbox.ShowDialog();
            return false;
        }
        return true;
    }
    #endregion Check for "untitled" entries in the list box

    #region Add new normal item
    /// <summary>
    /// Adds a new Normal item to the list
    /// </summary>
    private void NewItem()
    {
        Child newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            EntryType = (int)ListEntryType.Normal,
            ItemID = Guid.NewGuid().ToString()
        };
        Child.Children.Add(newitem);
        if (TvMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
        {
            tvi.IsSelected = true;
            tvi.BringIntoView();
        }
        _ = tbTitle.Focus();
        tbTitle.SelectAll();
        ClearAndQueueMessage("New \"untitled\" item was created.", 3000);
    }
    #endregion Add New item

    #region Add New Pop-Up
    /// <summary>
    /// Adds a new pop-up item to the list
    /// </summary>
    private void NewPopup()
    {
        Child newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            IconSource = "Menu.png",
            ChildrenOfChild = new ObservableCollection<Child>(),
            EntryType = ListEntryType.Popup,
            ItemID = Guid.NewGuid().ToString(),
        };
        Child.Children.Add(newitem);
        if (TvMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
        {
            tvi.IsSelected = true;
        }
        _ = tbTitle.Focus();
        tbTitle.SelectAll();
        ClearAndQueueMessage("New \"untitled\" pop-up list was created.", 3000);
    }
    #endregion Add New Pop-Up

    #region Delete an item
    /// <summary>
    /// Calls RemoveByID to delete an item from the list
    /// </summary>
    private void DeleteItem()
    {
        if (TvMaint.SelectedItem != null)
        {
            Child itemToDelete = (TvMaint.SelectedItem as Child);

            if (itemToDelete?.ChildrenOfChild is not null && itemToDelete.ChildrenOfChild.Count > 0)
            {
                MDCustMsgBox mbox = new($"Remove {itemToDelete.Title} and all {itemToDelete.ChildrenOfChild.Count} of its child items?",
                                        "Delete All?",
                                        ButtonType.YesNo,
                                        true,
                                        true,
                                        this,
                                        false);
                    mbox.ShowDialog();
                if (MDCustMsgBox.CustResult != CustResultType.Yes)
                {
                    return;
                }
            }
            RemoveByID(Child.Children, itemToDelete);
        }
        else
        {
            ClearAndQueueMessage("No item was selected to delete.", 3000);
        }
    }
    #endregion Delete an item

    #region Discard changes
    /// <summary>
    /// Discard any changes make since the last time the file was changed
    /// </summary>
    private void DiscardChanges()
    {
        JsonHelpers.ReadJson();
        (Application.Current.MainWindow as MainWindow)?.ResetListBox();
        Close();
    }
    #endregion Discard changes

    #region Remove an item from the list
    /// <summary>
    /// Removes a single item from the list.
    /// </summary>
    /// <param name="children">ObservableCollection to search</param>
    /// <param name="delItem">Child object to remove</param>
    private void RemoveByID(ObservableCollection<Child> children, Child delItem)
    {
        for (int i = children.Count - 1; i >= 0; --i)
        {
            Child child = children[i];

            if (child.ItemID == delItem.ItemID && child.Title == delItem.Title)
            {
                children.RemoveAt(i);
                log.Debug($"Removing \"{child.Title}\" - {child.ItemID}");
                ClearAndQueueMessage($"\"{child.Title}\" was removed.", 3000);
                EntriesChanged = true;
                break;
            }
            else if (child.ChildrenOfChild != null)
            {
                RemoveByID(child.ChildrenOfChild, delItem);
            }
        }
    }
    #endregion Remove an item from the list

    #region File picker buttons (for Path)
    private void BtnFilePicker_Click(object sender, RoutedEventArgs e)
    {
        ChooseFile();
    }
    private void BtnFolderPicker_Click(object sender, RoutedEventArgs e)
    {
        ChooseFolder();
    }

    /// <summary>
    /// Pick a file using the OpenFileDialog
    /// </summary>
    private void ChooseFile()
    {
        OpenFileDialog dlgOpen = new()
        {
            Title = "Browse for a File",
            Multiselect = false,
            CheckFileExists = false,
            CheckPathExists = true,
        };
        bool? result = dlgOpen.ShowDialog();
        if (result == true)
        {
            tbPath.Text = dlgOpen.FileName;
            Child entry = (Child)TvMaint.SelectedItem;
            entry.FilePathOrURI = tbPath.Text;
        }
    }

    /// <summary>
    /// Pick a folder using the Windows Forms FolderBrowserDialog
    /// </summary>
    private void ChooseFolder()
    {
        System.Windows.Forms.FolderBrowserDialog dialogFolder = new()
        {
            Description = "Browse for a Folder",
            UseDescriptionForTitle = true,
            AutoUpgradeEnabled = true,
        };

        if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            tbPath.Text = dialogFolder.SelectedPath;
            Child entry = (Child)TvMaint.SelectedItem;
            entry.FilePathOrURI = tbPath.Text;
        }
    }
    #endregion File picker buttons (for Path)

    #region Button events
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        DeleteItem();
    }

    private void NewItem_Click(object sender, RoutedEventArgs e)
    {
        if (CheckForUntitled())
        {
            pbxNewItem.IsPopupOpen = true;
        }
    }

    private void NewNormalItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        NewItem();
    }

    private void NewPopupItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        NewPopup();
    }

    private void NewSpecialApp_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxSpecialApps.IsPopupOpen = true;
    }

    private void Discard_Click(object sender, RoutedEventArgs e)
    {
        DiscardChanges();
    }

    private void BtnBackup_Click(object sender, RoutedEventArgs e)
    {
        JsonHelpers.CreateBackupFile();
    }

    private void BtnSaveClose_Click(object sender, RoutedEventArgs e)
    {
        JsonHelpers.SaveMainJson();
        (Application.Current.MainWindow as MainWindow)?.ResetListBox();
        Close();
    }

    private void CancelNewItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
    }
    #endregion Button events

    #region Move to next control on Enter
    private void Textbox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (e.Source is TextBox tbx)
            {
                tbx.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            e.Handled = true;
        }
    }
    #endregion Move to next control on Enter

    #region Mouse Enter/Leave Card (to change shadow)
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
    #endregion Mouse Enter/Leave Card (to change shadow)

    #region Add "Special" apps
    /// <summary>
    /// Add predefined "special" app as normal item
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BtnSpecial_Click(object sender, RoutedEventArgs e)
    {
        if (CheckForUntitled())
        {
            Child newitem = new();
            Button btn = sender as Button;
            switch (btn.Content.ToString())
            {
                case "Calculator":
                    newitem.Title = "Calculator";
                    newitem.FilePathOrURI = "calculator:";
                    newitem.IconSource = "calc.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Calendar":
                    newitem.Title = "Calendar";
                    newitem.FilePathOrURI = "outlookcal:";
                    newitem.IconSource = "calendar.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Email":
                    newitem.Title = "Email";
                    newitem.FilePathOrURI = "outlookmail:";
                    newitem.IconSource = "mail.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Solitaire":
                    newitem.Title = "Solitaire Collection";
                    newitem.FilePathOrURI = "xboxliveapp-1297287741:";
                    newitem.IconSource = "cards.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Weather":
                    newitem.Title = "Weather";
                    newitem.FilePathOrURI = "bingweather:";
                    newitem.IconSource = "weather.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Windows Settings":
                    newitem.Title = "Windows Settings";
                    newitem.FilePathOrURI = "ms-settings:";
                    newitem.IconSource = "gear.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Restart":
                    newitem.Title = "Restart (Immediate)";
                    newitem.FilePathOrURI = "shutdown.exe";
                    newitem.Arguments = "/r /t 0";
                    newitem.IconSource = "restart.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
                case "Shutdown":
                    newitem.Title = "Shutdown (Immediate)";
                    newitem.FilePathOrURI = "shutdown.exe";
                    newitem.Arguments = "/s /t 0";
                    newitem.IconSource = "shutdown.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    break;
            }
            if (newitem.Title != string.Empty)
            {
                newitem.ItemID = Guid.NewGuid().ToString();
                Child.Children.Add(newitem);
                EntriesChanged = true;
                if (TvMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
                {
                    tvi.IsSelected = true;
                    tvi.BringIntoView();
                }
                _ = tbTitle.Focus();
                tbTitle.SelectAll();
                ClearAndQueueMessage($"New \"{newitem.Title}\" item was created.", 3000);
            }
        }
    }
    #endregion Add "Special" apps

    #region Get an image for the selected item
    /// <summary>
    /// File picker for image
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void IconFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dlgOpen = new()
        {
            Title = "Choose an Image File",
            InitialDirectory = Path.Combine(AppInfo.AppDirectory, "Icons"),
            Filter = "Image Files|*.png;*.ico;*.bmp;*.jpg;*.jpeg",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true
        };
        bool? result = dlgOpen.ShowDialog();
        if (result == true && File.Exists(dlgOpen.FileName))
        {
            if (Path.GetDirectoryName(dlgOpen.FileName) == dlgOpen.InitialDirectory)
            {
                tbIconFile.Text = Path.GetFileName(dlgOpen.FileName);
                Child entry = (Child)TvMaint.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
            else
            {
                tbIconFile.Text = dlgOpen.FileName;
                Child entry = (Child)TvMaint.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
        }
    }
    #endregion Get an image for the selected item

    #region Collection changed event
    private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        EntriesChanged = true;
        Debug.WriteLine($"List changed, action was: {e.Action} {e.OldStartingIndex} {e.NewStartingIndex}");
    }
    #endregion Collection changed event

    #region Clear message queue then queue a snackbar message and set duration
    /// <summary>
    /// Displays a snackbar message in the Maintenance window
    /// </summary>
    /// <param name="message">Text of the message</param>
    /// <param name="duration">Time in milliseconds to display the message</param>
    public void ClearAndQueueMessage(string message, int duration)
    {
        SnackBarMaint.MessageQueue.Clear();
        SnackBarMaint.MessageQueue.Enqueue(message,
            null,
            null,
            null,
            false,
            true,
            TimeSpan.FromMilliseconds(duration));
    }
    #endregion Clear message queue then queue a snackbar message and set duration

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

    private void BtnData_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(JsonHelpers.GetMainListFile());
    }

    private void BtnFolder_Click(object sender, RoutedEventArgs e)
    {
        Child child = new()
        {
            Title = "App Folder",
            FilePathOrURI = AppInfo.AppDirectory
        };
        MainWindow.LaunchApp(child);
    }
}
