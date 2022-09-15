// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

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

        LoadPopups();
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

        // Menu items collection changed
        MyMenuItem.MLMenuItems.CollectionChanged += MLMenuItems_CollectionChanged;
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

    #region Load the ComboBox
    /// <summary>
    /// Loads the ComboBox with the list of pop-ups
    /// </summary>
    private void LoadPopups()
    {
        cbxPopups.ItemsSource = PopupHelpers.SortedPopups();
    }
    #endregion Load the ComboBox

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
            ItemID = Guid.NewGuid().ToString(),
            IsSelected = true
        };
        AddNewItem(newitem);
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
            IsSelected = true,
            SubMenuItems = new ObservableCollection<MyMenuItem>()
        };
        AddNewItem(newitem);
        ClearAndQueueMessage("New \"untitled\" submenu was created.", 3000);
    }
    #endregion Add new submenu

    #region Add new section heading
    /// <summary>
    /// Adds a new section heading item
    /// </summary>
    private void NewSectionHead()
    {
        MyMenuItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            ItemType = MenuItemType.SectionHead,
            ItemID = Guid.NewGuid().ToString(),
            IsSelected = true,
            SubMenuItems = null
        };
        AddNewItem(newitem);
        ClearAndQueueMessage("New \"untitled\" section heading was created.", 3000);
    }
    #endregion Add new section heading

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
            IsSelected = true,
            SubMenuItems = null
        };
        AddNewItem(newitem);
        ClearAndQueueMessage("New Separator item was created.", 3000);
    }
    #endregion Add new separator

    #region Add Existing Pop-up
    /// <summary>
    /// Adds a new pop-up item
    /// </summary>
    private void AddPopup()
    {
        MyMenuItem newitem = new()
        {
            Title = "untitled",
            FilePathOrURI = string.Empty,
            Arguments = string.Empty,
            ItemType = MenuItemType.Popup,
            ItemID = Guid.NewGuid().ToString(),
            PopupID = string.Empty,
            IsSelected = true,
            SubMenuItems = null
        };
        AddNewItem(newitem);
        ClearAndQueueMessage("New \"untitled\" pop-up was created.", 3000);
    }
    #endregion Add Existing Pop-up

    #region Add the new item to the list
    /// <summary>
    /// Adds the new item to the desired position in the list
    /// </summary>
    /// <param name="newitem">The new item</param>
    private void AddNewItem(MyMenuItem newitem)
    {
        if (TvMenuMaint.SelectedItem is not null)
        {
            MyMenuItem selectedItem = TvMenuMaint.SelectedItem as MyMenuItem;
            Debug.WriteLine($"Selected item is: {selectedItem.Title}");
            if (rbNewAbove.IsChecked == true)
            {
                InsertInList(MyMenuItem.MLMenuItems, selectedItem, newitem, true);
            }
            else if (rbNewBelow.IsChecked == true)
            {
                InsertInList(MyMenuItem.MLMenuItems, selectedItem, newitem, false);
            }
            else
            {
                MyMenuItem.MLMenuItems.Add(newitem);
            }
        }
        else
        {
            MyMenuItem.MLMenuItems.Add(newitem);
        }
        if (newitem.ItemType != MenuItemType.Separator)
        {
            _ = tbTitle.Focus();
        }
    }
    #endregion Add the new item to the list

    #region Insert new item into the list
    /// <summary>
    /// Inserts a new menu item the in list of menu items.
    /// </summary>
    /// <param name="menuItems">The ObservableCollection of menu items.</param>
    /// <param name="selectedItem">The TreeView item that is selected.</param>
    /// <param name="newItem">The new item.</param>
    /// <param name="above">if set to <c>true</c> insert new item above the selected item. Otherwise insert the new item below the selected item</param>
    private void InsertInList(ObservableCollection<MyMenuItem> menuItems, MyMenuItem selectedItem, MyMenuItem newItem, bool above)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            MyMenuItem item = menuItems[i];

            if (item.ItemID == selectedItem.ItemID)
            {
                if (above)
                {
                    menuItems.Insert(i, newItem);
                }
                else
                {
                    menuItems.Insert(i + 1, newItem);
                }
                break;
            }
            else if (item.SubMenuItems != null)
            {
                InsertInList(item.SubMenuItems, selectedItem, newItem, above);
            }
        }
    }
    #endregion Insert new item into the list

    #region Delete an item
    /// <summary>
    /// Calls RemoveByID to delete an item from the list
    /// </summary>
    private void DeleteItem()
    {
        if (TvMenuMaint.SelectedItem != null)
        {
            MyMenuItem itemToDelete = TvMenuMaint.SelectedItem as MyMenuItem;

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
    /// <param name="menuItems">ObservableCollection to search</param>
    /// <param name="delItem">MyListItem object to remove</param>
    private void RemoveByID(ObservableCollection<MyMenuItem> menuItems, MyMenuItem delItem)
    {
        for (int i = menuItems.Count - 1; i >= 0; --i)
        {
            MyMenuItem item = menuItems[i];

            if (item.ItemID == delItem.ItemID && item.Title == delItem.Title)
            {
                menuItems.RemoveAt(i);
                log.Debug($"Removing \"{item.Title}\" - {item.ItemID}");
                ClearAndQueueMessage($"\"{item.Title}\" was removed.", 3000);
                break;
            }
            else if (item.SubMenuItems != null)
            {
                RemoveByID(item.SubMenuItems, delItem);
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

    #region TreeView events
    /// <summary>
    /// TreeView item selected event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TvMenuMaint_Selected(object sender, RoutedEventArgs e)
    {
        TreeViewItem tvi = e.OriginalSource as TreeViewItem;
        tvi?.BringIntoView();

        if (tbTitle.Text.Equals("untitled", StringComparison.OrdinalIgnoreCase))
        {
            tbTitle.Dispatcher.BeginInvoke(new Action(() => tbTitle.SelectAll()));
        }
    }
    #endregion TreeView events

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

    #region File & Folder picker buttons
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

    /// <summary>
    /// Pick a folder for working directory
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
            MyMenuItem entry = (MyMenuItem)TvMenuMaint.SelectedItem;
            entry.WorkingDir = tbWorkDir.Text;
        }
    }
    #endregion File & Folder picker buttons

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
        pbxNewItem.IsPopupOpen = false;
        NewMenuItem();
    }

    private void NewSubMenu_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        NewSubMenu();
    }

    private void NewSectionHead_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        NewSectionHead();
    }

    private void NewSeparator_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        NewSeparator();
    }

    private void Popup_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
        cbxPopups.IsDropDownOpen = true;
        AddPopup();
    }

    private void CancelNewItem_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        pbxNewItem.IsPopupOpen = false;
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

    private void BtnImport_Click(object sender, RoutedEventArgs e)
    {
        if (JsonHelpers.ImportMenuFile())
        {
            JsonHelpers.SaveMenuJson();
            LoadTreeView();
            (Application.Current.MainWindow as MainWindow)?.PopulateTrayMenu();
        }
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

    private void BtnFolder_Click(object sender, RoutedEventArgs e)
    {
        MyListItem item = new()
        {
            Title = "App Folder",
            FilePathOrURI = AppInfo.AppDirectory
        };
        MainWindow.LaunchApp(item);
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

    private void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        if (TvMenuMaint.SelectedItem is MyMenuItem menuItem)
        {
            if (menuItem is not null && (menuItem.ItemType == MenuItemType.MenuItem))
            {
                MyListItem item = new()
                {
                    FilePathOrURI = menuItem.FilePathOrURI,
                    Arguments = menuItem.Arguments,
                    Title = menuItem.Title,
                    WorkingDir = menuItem.WorkingDir,
                    RunElevated = menuItem.RunElevated,
                };
                _ = MainWindow.LaunchApp(item);
            }
            if (menuItem is not null && menuItem.ItemType == MenuItemType.Popup)
            {
                // Show Pop-up
                if (menuItem.ItemType == MenuItemType.Popup)
                {
                    MyListItem pop = PopupHelpers.FindPopup(MyListItem.Children, menuItem.PopupID);
                    if (pop == null)
                    {
                        log.Debug($"Couldn't find Pop-up with ID: {menuItem.PopupID}");
                        return;
                    }
                    if (pop.EntryType != ListEntryType.Popup)
                    {
                        log.Debug($"{menuItem.PopupID} is not a Pop-up ID");
                        return;
                    }
                    MainWindow.OpenPopup(pop);
                }
            }
        }
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

    #region Pop-ups ComboBox selection changed
    /// <summary>
    /// ComboBox selection changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CbxPopups_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbxPopups.SelectedItem != null
            && IsLoaded
            && TvMenuMaint.SelectedItem is MyMenuItem myMenuItem
            && myMenuItem.ItemType == MenuItemType.Popup)
        {
            myMenuItem.PopupID = (cbxPopups.SelectedItem as MyListItem)?.ItemID;
        }
    }
    #endregion Pop-ups ComboBox selection changed

    #region Menu items collection changed
    /// <summary>
    /// Handles the CollectionChanged event of the MLMenuItems control.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">instance containing the event data</param>
    private void MLMenuItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine($"List changed, action was: {e.Action} {e.OldStartingIndex} {e.NewStartingIndex}");
    }
    #endregion Menu items collection changed

    #region Drag and Drop handlers for TextBoxes
    /// <summary>
    /// Handles the PreviewDragOver event of the TextBox_AnyType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The instance containing the event data.</param>
    private void TextBox_AnyType_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            List<string> dragfiles = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList();
            e.Effects = dragfiles?.Count == 1 ? DragDropEffects.Copy : DragDropEffects.None;
        }
        else if (e.Data.GetDataPresent(DataFormats.Text))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Handles the PreviewDragOver event of the TextBox_OnlyDirectory control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The instance containing the event data.</param>
    private void TextBox_OnlyDirectory_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            List<string> dragfiles = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList();
            FileAttributes attr = File.GetAttributes(dragfiles.FirstOrDefault());
            e.Effects = dragfiles?.Count == 1 && attr.HasFlag(FileAttributes.Directory) ? DragDropEffects.Copy : DragDropEffects.None;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Handles the PreviewDrop event of the TextBox controls.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The instance containing the event data.</param>
    private void TextBox_PreviewDrop(object sender, DragEventArgs e)
    {
        if (e.Data is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            TextBox tbox = sender as TextBox;
            tbox.Text = string.Empty;
            tbox.Text = ((DataObject)e.Data).GetFileDropList().Cast<string>().ToList().FirstOrDefault();
            BindingExpression be = (sender as TextBox)?.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
        else if (e.Data is not null && e.Data.GetDataPresent(DataFormats.Text))
        {
            TextBox tbox = sender as TextBox;
            tbox.Text = string.Empty;
            tbox.Text = ((DataObject)e.Data).GetText();
            BindingExpression be = (sender as TextBox)?.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
        e.Handled = true;
    }
    #endregion Drag and Drop handlers for TextBoxes
}