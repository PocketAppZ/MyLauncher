// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Interaction logic for MenuMaint.xaml
/// </summary>
public partial class MenuMaint : Window
{
    #region NLog
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog

    public MenuMaint()
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
        if (UserSettings.Setting.MenuMaintFistRun)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            UserSettings.Setting.MenuMaintFistRun = false;
        }
        else
        {
            // Window position
            Top = UserSettings.Setting.MenuMaintWindowTop;
            Left = UserSettings.Setting.MenuMaintWindowLeft;
        }
        Height = UserSettings.Setting.MenuMaintWindowHeight;
        Width = UserSettings.Setting.MenuMaintWindowWidth;

        // UI size
        double size = MainWindow.UIScale((MySize)UserSettings.Setting.UISize);
        MenuMaintGrid.LayoutTransform = new ScaleTransform(size, size);

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;

        MyMenuItem.MLMenuItems.CollectionChanged += MLMenuItems_CollectionChanged;
    }

    private void MLMenuItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine($"List changed, action was: {e.Action} {e.OldStartingIndex} {e.NewStartingIndex}");
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
                MenuMaintGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting changed

    #region Load the TreeView
    /// <summary>
    /// Loads the TreeView with the contents of the Observable Collection
    /// </summary>
    private void LoadTreeView()
    {
        if (MyMenuItem.MLMenuItems is not null)
        {
            TvMenuMaint.ItemsSource = MyMenuItem.MLMenuItems;
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
        if (MyMenuItem.MLMenuItems.Any(x => string.Equals(x.Title, "untitled", StringComparison.OrdinalIgnoreCase)))
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
    /// Adds a new menu item
    /// </summary>
    private void NewMenuItem()
    {
        MyMenuItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            ItemType = MenuItemType.MenuItem,
            ItemID = Guid.NewGuid().ToString()
        };
        MyMenuItem.MLMenuItems.Add(newitem);
        if (TvMenuMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
        {
            tvi.IsSelected = true;
            tvi.BringIntoView();
        }
        _ = tbTitle.Focus();
        tbTitle.SelectAll();
        ClearAndQueueMessage("New \"untitled\" item was created.", 3000);
    }
    #endregion Add new normal item

    #region Add new submenu
    /// <summary>
    /// Adds a new submenu item
    /// </summary>
    private void NewSubMenu()
    {
        MyMenuItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            ItemType = MenuItemType.SubMenu,
            ItemID = Guid.NewGuid().ToString(),
            SubMenuItems = new ObservableCollection<MyMenuItem>()
        };
        MyMenuItem.MLMenuItems.Add(newitem);
        if (TvMenuMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
        {
            tvi.IsSelected = true;
            tvi.BringIntoView();
        }
        _ = tbTitle.Focus();
        tbTitle.SelectAll();
        ClearAndQueueMessage("New \"untitled\" item was created.", 3000);
    }
    #endregion Add new submenu

    #region Add new separator
    /// <summary>
    /// Adds a new submenu item
    /// </summary>
    private void NewSeparator()
    {
        MyMenuItem newitem = new()
        {
            Title = "*️Separator*",
            FilePathOrURI = string.Empty,
            ItemType = MenuItemType.Separator,
            ItemID = Guid.NewGuid().ToString(),
            SubMenuItems = null
        };
        MyMenuItem.MLMenuItems.Add(newitem);
        if (TvMenuMaint.ItemContainerGenerator.ContainerFromItem(newitem) is TreeViewItem tvi)
        {
            tvi.IsSelected = true;
            tvi.BringIntoView();
        }
        _ = tbTitle.Focus();
        ClearAndQueueMessage("New Separator item was created.", 3000);
    }
    #endregion Add new separator

    #region Delete an item
    /// <summary>
    /// Calls RemoveByID to delete an item from the list
    /// </summary>
    private void DeleteItem()
    {
        if (TvMenuMaint.SelectedItem != null)
        {
            MyMenuItem itemToDelete = (TvMenuMaint.SelectedItem as MyMenuItem);

            if (itemToDelete?.SubMenuItems is not null && itemToDelete.SubMenuItems.Count > 0)
            {
                MDCustMsgBox mbox = new($"Remove {itemToDelete.Title} and all {itemToDelete.SubMenuItems.Count} of its child items?",
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
            RemoveByID(MyMenuItem.MLMenuItems, itemToDelete);
        }
        else
        {
            ClearAndQueueMessage("No item was selected to delete.", 3000);
        }
    }
    #endregion Delete an item

    #region Remove an item from the list
    /// <summary>
    /// Removes a single item from the list.
    /// </summary>
    /// <param name="children">ObservableCollection to search</param>
    /// <param name="delItem">Child object to remove</param>
    private void RemoveByID(ObservableCollection<MyMenuItem> children, MyMenuItem delItem)
    {
        for (int i = children.Count - 1; i >= 0; --i)
        {
            MyMenuItem child = children[i];

            if (child.ItemID == delItem.ItemID && child.Title == delItem.Title)
            {
                children.RemoveAt(i);
                log.Debug($"Removing \"{child.Title}\" - {child.ItemID}");
                ClearAndQueueMessage($"\"{child.Title}\" was removed.", 3000);
                break;
            }
            else if (child.SubMenuItems != null)
            {
                RemoveByID(child.SubMenuItems, delItem);
            }
        }
    }
    #endregion Remove an item from the list

    #region Window events
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //Select the first item in the TreeView
        _ = TvMenuMaint.Focus();
        if (TvMenuMaint.Items.Count > 0)
        {
            MyMenuItem first = MyMenuItem.MLMenuItems.FirstOrDefault();
            if (TvMenuMaint.ItemContainerGenerator.ContainerFromItem(first) is TreeViewItem tvi)
            {
                tvi.IsSelected = true;
            }
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        // Clear any remaining messages
        SnackBarMenuMaint.MessageQueue.Clear();

        // Save window position
        UserSettings.Setting.MenuMaintWindowLeft = Math.Floor(Left);
        UserSettings.Setting.MenuMaintWindowTop = Math.Floor(Top);
        UserSettings.Setting.MenuMaintWindowWidth = Math.Floor(Width);
        UserSettings.Setting.MenuMaintWindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window events

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
            MyMenuItem entry = (MyMenuItem)TvMenuMaint.SelectedItem;
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
            MyMenuItem entry = (MyMenuItem)TvMenuMaint.SelectedItem;
            entry.FilePathOrURI = tbPath.Text;
        }
    }
    #endregion File picker buttons (for Path)

    #region New items
    private void NewItem_Click(object sender, RoutedEventArgs e)
    {
        if (CheckForUntitled())
        {
            pbxNewItem.IsPopupOpen = true;
        }
    }

    private void NewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        NewMenuItem();
    }

    private void NewSubMenu_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        NewSubMenu();
    }

    private void NewSeparator_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        NewSeparator();
    }

    private void CancelNewItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }
    #endregion New items

    #region Button Events
    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        DeleteItem();
    }

    private void Discard_Click(object sender, RoutedEventArgs e)
    {
        DiscardChanges();
    }

    private void BtnBackup_Click(object sender, RoutedEventArgs e)
    {
        JsonHelpers.CreateMenuBackup();
    }

    private void DiscardChanges()
    {
        JsonHelpers.ReadMenuJson();
        (Application.Current.MainWindow as MainWindow)?.ResetTrayMenu();
        Close();
    }

    private void BtnSaveClose_Click(object sender, RoutedEventArgs e)
    {
        JsonHelpers.SaveMenuJson();
        (Application.Current.MainWindow as MainWindow)?.ResetTrayMenu();
        Close();
    }

    private void BtnData_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(JsonHelpers.GetMenuListFile());
    }
    #endregion Button Events

    #region Clear message queue then queue a snackbar message and set duration
    /// <summary>
    /// Displays a snackbar message in the Maintenance window
    /// </summary>
    /// <param name="message">Text of the message</param>
    /// <param name="duration">Time in milliseconds to display the message</param>
    public void ClearAndQueueMessage(string message, int duration)
    {
        SnackBarMenuMaint.MessageQueue.Clear();
        SnackBarMenuMaint.MessageQueue.Enqueue(message,
            null,
            null,
            null,
            false,
            true,
            TimeSpan.FromMilliseconds(duration));
    }
    #endregion Clear message queue then queue a snackbar message and set duration

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
