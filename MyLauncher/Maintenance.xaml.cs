// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class Maintenance : Window
{
    #region NLog
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog

    public Maintenance()
    {
        InitializeComponent();

        InitSettings();

        LoadTreeView();
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

        //MyListItem.Entries.ListChanged += Entries_ListChanged;
        MyListItem.Children.CollectionChanged += Children_CollectionChanged;
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
            MyListItem first = MyListItem.Children.FirstOrDefault();
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

    #region TreeView events
    private void TvMaint_Selected(object sender, RoutedEventArgs e)
    {
        TreeViewItem tvi = e.OriginalSource as TreeViewItem;
        tvi?.BringIntoView();
        if (tbTitle.Text.Equals("untitled", StringComparison.OrdinalIgnoreCase))
        {
            tbTitle.Dispatcher.BeginInvoke(new Action(() => tbTitle.SelectAll()));
        }
    }
    #endregion TreeView events

    #region Load the TreeView
    /// <summary>
    /// Loads the TreeView with the contents of the Observable Collection
    /// </summary>
    private void LoadTreeView()
    {
        if (MyListItem.Children is not null)
        {
            TvMaint.ItemsSource = MyListItem.Children;
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
        if (MyListItem.Children.Any(x => string.Equals(x.Title, "untitled", StringComparison.OrdinalIgnoreCase)))
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
        MyListItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            EntryType = (int)ListEntryType.Normal,
            ItemID = Guid.NewGuid().ToString(),
            IsSelected = true,
        };
        MyListItem.Children.Add(newitem);
        _ = tbTitle.Focus();
        ClearAndQueueMessage("New \"untitled\" item was created.", 3000);
    }
    #endregion Add New item

    #region Add New Pop-Up
    /// <summary>
    /// Adds a new pop-up item to the list
    /// </summary>
    private void NewPopup()
    {
        MyListItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            IconSource = "Menu.png",
            MyListItems = new ObservableCollection<MyListItem>(),
            EntryType = ListEntryType.Popup,
            ItemID = Guid.NewGuid().ToString(),
            IsSelected = true,
        };
        MyListItem.Children.Add(newitem);
        _ = tbTitle.Focus();
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
            MyListItem itemToDelete = (TvMaint.SelectedItem as MyListItem);

            if (itemToDelete?.MyListItems is not null && itemToDelete.MyListItems.Count > 0)
            {
                MDCustMsgBox mbox = new($"Remove {itemToDelete.Title} and all {itemToDelete.MyListItems.Count} of its item items?",
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
            RemoveByID(MyListItem.Children, itemToDelete);
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
    /// <param name="listItems">ObservableCollection to search</param>
    /// <param name="delItem">MyListItem object to remove</param>
    private void RemoveByID(ObservableCollection<MyListItem> listItems, MyListItem delItem)
    {
        for (int i = listItems.Count - 1; i >= 0; --i)
        {
            MyListItem item = listItems[i];

            if (item.ItemID == delItem.ItemID && item.Title == delItem.Title)
            {
                listItems.RemoveAt(i);
                log.Debug($"Removing \"{item.Title}\" - {item.ItemID}");
                ClearAndQueueMessage($"\"{item.Title}\" was removed.", 3000);
                break;
            }
            else if (item.MyListItems != null)
            {
                RemoveByID(item.MyListItems, delItem);
            }
        }
    }
    #endregion Remove an item from the list

    #region File picker buttons
    private void BtnFilePicker_Click(object sender, RoutedEventArgs e)
    {
        ChooseFile();
    }
    private void BtnFolderPicker_Click(object sender, RoutedEventArgs e)
    {
        ChooseFolder();
    }
    private void BtnWorkingDirPicker_Click(object sender, RoutedEventArgs e)
    {
        ChooseWorkDir();
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
            MyListItem entry = (MyListItem)TvMaint.SelectedItem;
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
            MyListItem entry = (MyListItem)TvMaint.SelectedItem;
            entry.FilePathOrURI = tbPath.Text;
        }
    }

    /// <summary>
    /// Folder picker for working directory
    /// </summary>
    private void ChooseWorkDir()
    {
        System.Windows.Forms.FolderBrowserDialog dialogFolder = new()
        {
            Description = "Browse for a Folder",
            UseDescriptionForTitle = true,
            AutoUpgradeEnabled = true,
        };

        if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            tbWorkDir.Text = dialogFolder.SelectedPath;
            MyListItem entry = (MyListItem)TvMaint.SelectedItem;
            entry.WorkingDir = tbWorkDir.Text;
        }
    }
    #endregion File picker buttons

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

    private void BtnData_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(JsonHelpers.GetMainListFile());
    }

    private void BtnFolder_Click(object sender, RoutedEventArgs e)
    {
        MyListItem item = new()
        {
            Title = "App Folder",
            FilePathOrURI = AppInfo.AppDirectory
        };
        MainWindow.LaunchApp(item);
    }

    private void BtnImport_Click(object sender, RoutedEventArgs e)
    {
        if (JsonHelpers.ImportListFile())
        {
            JsonHelpers.SaveMainJson();
            LoadTreeView();
            (Application.Current.MainWindow as MainWindow)?.PopulateMainListBox();
        }
    }

    private void AdminToggleButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Controls.Primitives.ToggleButton tb = sender as System.Windows.Controls.Primitives.ToggleButton;
        if ((bool)tb.IsChecked)
        {
            ClearAndQueueMessage("Will run as Administrator", 2000);
        }
        else
        {
            ClearAndQueueMessage("Will not run as Administrator", 2000);
        }
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
            MyListItem newitem = new();
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
                newitem.IsSelected = true;
                MyListItem.Children.Add(newitem);
                _ = tbTitle.Focus();
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
                MyListItem entry = (MyListItem)TvMaint.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
            else
            {
                tbIconFile.Text = dlgOpen.FileName;
                MyListItem entry = (MyListItem)TvMaint.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
        }
    }
    #endregion Get an image for the selected item

    #region Collection changed event
    private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
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
}
