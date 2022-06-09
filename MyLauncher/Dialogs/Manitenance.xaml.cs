// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher.Dialogs;
public partial class Maintenance : UserControl
{
    #region NLog
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog

    #region Properties
    public static bool EntriesChanged { get; set; }
    public static object PrevEntry { get; set; }
    #endregion Properties

    public Maintenance()
    {
        InitializeComponent();

        LoadListBox();

        LoadComboBox();

        EntryClass.Entries.ListChanged += Entries_ListChanged;
    }
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _ = listbox1.Focus();
    }

    #region List changed event
    private void Entries_ListChanged(object sender, ListChangedEventArgs e)
    {
        EntriesChanged = true;
        btnDiscard.IsEnabled = true;
    }
    #endregion List changed event

    #region Load the ListBox
    private void LoadListBox()
    {
        if (EntryClass.Entries.Count > 0)
        {
            listbox1.ItemsSource = EntryClass.Entries;
            listbox1.SelectedItem = EntryClass.Entries.First();
            _ = listbox1.Focus();
        }
    }
    #endregion Load the ListBox

    #region Load the ComboBox
    private void LoadComboBox()
    {
        BindingList<EntryClass> hostEntries = new();

        EntryClass entry1 = new()
        {
            HostID = 0,
            Title = "Main Page"
        };
        hostEntries.Add(entry1);

        foreach (EntryClass entry in EntryClass.Entries)
        {
            if (entry.EntryType == 1)
            {
                hostEntries.Add(entry);
            }
        }

        HostCombo.ItemsSource = hostEntries;
    }
    #endregion Load the ComboBox

    #region Check for "untitled" entries in the list box
    private bool CheckForUntitled()
    {
        // Loop through the list backwards checking for null titles
        for (int i = listbox1.Items.Count - 1; i >= 0; i--)
        {
            object item = listbox1.Items[i];
            EntryClass x = item as EntryClass;
            if (string.IsNullOrEmpty(x.Title))
            {
                log.Error("New item prohibited, \"untitled\" entry in list");
                _ = new MDCustMsgBox("Please update or delete the \"untitled\" entry before adding another new entry.",
                    "ERROR", ButtonType.Ok).ShowDialog();
                return false;
            }
        }
        return true;
    }
    #endregion Check for "untitled" entries in the list box

    #region Add New item
    private void NewItem()
    {
        EntryClass newitem = new()
        {
            Title = string.Empty,
            FilePathOrURI = String.Empty,
            EntryType = 0,
            HostID = -1
        };
        EntryClass.Entries.Add(newitem);
        listbox1.SelectedIndex = listbox1.Items.Count - 1;
        listbox1.ScrollIntoView(listbox1.SelectedItem);
        _ = tbTitle.Focus();
        SnackbarMsg.ClearAndQueueMessage("New \"untitled\" item was created.", 5000);
    }
    #endregion Add New item

    #region Add New Pop-Up
    private void NewPopup()
    {
        EntryClass newitem = new()
        {
            Title = string.Empty,
            FilePathOrURI = string.Empty,
            IconSource = "Menu.png",
            EntryType = (int)ListEntryType.Popup,
            HostID = ++UserSettings.Setting.LastPopID
        };
        EntryClass.Entries.Add(newitem);
        listbox1.SelectedIndex = listbox1.Items.Count - 1;
        listbox1.ScrollIntoView(listbox1.SelectedItem);
        _ = tbTitle.Focus();
        SnackbarMsg.ClearAndQueueMessage("New \"untitled\" pop-up list was created.", 5000);
    }
    #endregion Add New Pop-Up

    #region Delete an item
    private void DeleteItem()
    {
        if (listbox1.SelectedItem != null)
        {
            string item = (listbox1.SelectedItem as EntryClass)?.Title;
            int index = listbox1.SelectedIndex;
            _ = EntryClass.Entries.Remove((EntryClass)listbox1.SelectedItem);
            SnackbarMsg.ClearAndQueueMessage($"Deleted \"{item}\"", 2000);
            if (index > 0)
            {
                listbox1.SelectedItem = listbox1.Items[index - 1];
                listbox1.ScrollIntoView(listbox1.SelectedItem);
            }
            else
            {
                listbox1.SelectedItem = EntryClass.Entries.First();
            }
            _ = listbox1.Focus();
        }
        else
        {
            SnackbarMsg.ClearAndQueueMessage("No item was selected to delete.", 5000);
        }
    }
    #endregion Delete an item

    #region Save the list to JSON file
    private static async void SaveJson()
    {
        List<EntryClass> tempCollection = new();

        foreach (EntryClass item in EntryClass.Entries)
        {
            if (!string.IsNullOrEmpty(item.Title))
            {
                EntryClass ec = new()
                {
                    Title = item.Title.Trim(),
                    FilePathOrURI = item.FilePathOrURI.Trim('"').Trim(),
                    FileIcon = item.FileIcon,
                    Arguments = item.Arguments,
                    IconSource = item.IconSource.Trim(),
                    EntryType = item.EntryType,
                    HostID = item.HostID,
                    ChildOfHost = item.ChildOfHost
                };
                tempCollection.Add(ec);
            }
        }

        JsonSerializerOptions opts = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true
        };

        try
        {
            log.Info($"Saving JSON file: {GetJsonFile()}");
            string json = JsonSerializer.Serialize(tempCollection, opts);
            File.WriteAllText(GetJsonFile(), json);
            EntriesChanged = false;
            SnackbarMsg.QueueMessage("File Saved.", 2000);
            tempCollection.Clear();
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error saving file.");
            SystemSounds.Exclamation.Play();
            ErrorDialog error = new()
            {
                Message = $"Error saving file\n\n{ex.Message}"
            };
            _ = await DialogHost.Show(error, "MainDialogHost");
        }
    }
    #endregion Save the list to JSON file

    #region Get the JSON file name
    private static string GetJsonFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncher.json");
    }
    #endregion Get the JSON file name

    #region File picker buttons (for Path)
    private void BtnFilePicker_Click(object sender, RoutedEventArgs e)
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
            EntryClass entry = (EntryClass)listbox1.SelectedItem;
            entry.FilePathOrURI = tbPath.Text;
        }
    }

    private void BtnFolderPicker_Click(object sender, RoutedEventArgs e)
    {
        using System.Windows.Forms.FolderBrowserDialog dialogFolder = new()
        {
            Description = "Browse for a Folder",
            UseDescriptionForTitle = true,
            AutoUpgradeEnabled = true,
        };

        if (dialogFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            tbPath.Text = dialogFolder.SelectedPath;
            EntryClass entry = (EntryClass)listbox1.SelectedItem;
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
        MainWindow.ReadJson();
        (Application.Current.MainWindow as MainWindow)?.ResetListBox();
        LoadListBox();
        listbox1.Items.Refresh();
        EntriesChanged = false;
        btnDiscard.IsEnabled = false;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        DialogHost.Close("MainDialogHost");
        if (EntriesChanged)
        {
            SaveJson();
            (Application.Current.MainWindow as MainWindow)?.ResetListBox();
            SnackbarMsg.ClearAndQueueMessage("List saved", 1000);
        }
        else
        {
            Debug.WriteLine("list wasn't saved");
        }
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
    private void BtnSpecial_Click(object sender, RoutedEventArgs e)
    {
        if (CheckForUntitled())
        {
            EntryClass newitem = new();
            Button btn = sender as Button;
            switch (btn.Content.ToString())
            {
                case "Calculator":
                    newitem.Title = "Calculator";
                    newitem.FilePathOrURI = "calculator:";
                    newitem.IconSource = "calc.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Calendar":
                    newitem.Title = "Calendar";
                    newitem.FilePathOrURI = "outlookcal:";
                    newitem.IconSource = "calendar.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Email":
                    newitem.Title = "Email";
                    newitem.FilePathOrURI = "outlookmail:";
                    newitem.IconSource = "mail.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Solitaire":
                    newitem.Title = "Solitaire Collection";
                    newitem.FilePathOrURI = "xboxliveapp-1297287741:";
                    newitem.IconSource = "cards.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Weather":
                    newitem.Title = "Weather";
                    newitem.FilePathOrURI = "bingweather:";
                    newitem.IconSource = "weather.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Windows Settings":
                    newitem.Title = "Windows Settings";
                    newitem.FilePathOrURI = "ms-settings:";
                    newitem.IconSource = "gear.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Restart":
                    newitem.Title = "Restart";
                    newitem.FilePathOrURI = "shutdown.exe";
                    newitem.Arguments = "/r /t 0";
                    newitem.IconSource = "restart.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
                case "Shutdown":
                    newitem.Title = "Shutdown";
                    newitem.FilePathOrURI = "shutdown.exe";
                    newitem.Arguments = "/s /t 0";
                    newitem.IconSource = "shutdown.png";
                    newitem.EntryType = (int)ListEntryType.Normal;
                    newitem.HostID = 0;
                    break;
            }
            if (tbTitle.Text != string.Empty)
            {
                EntryClass.Entries.Add(newitem);
                listbox1.SelectedIndex = listbox1.Items.Count - 1;
                listbox1.ScrollIntoView(listbox1.SelectedItem);
                _ = listbox1.Focus();
                EntriesChanged = true;
            }
        }
    }
    #endregion Add "special" app

    #region Get an image for the selected item
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
                EntryClass entry = (EntryClass)listbox1.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
            else
            {
                tbIconFile.Text = dlgOpen.FileName;
                EntryClass entry = (EntryClass)listbox1.SelectedItem;
                entry.IconSource = tbIconFile.Text;
            }
        }
    }
    #endregion Get an image for the selected item

    #region ListBox selection changed
    private void Listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (listbox1.SelectedItem == null)
        {
            listbox1.SelectedItem = PrevEntry;
            return;
        }
        PrevEntry = listbox1.SelectedItem;

        if (listbox1.SelectedItem != null)
        {
            int? x = (listbox1.SelectedItem as EntryClass)?.EntryType;
            tbPath.IsEnabled = x != 1;
            tbArgs.IsEnabled = x != 1;
        }
    }
    #endregion ListBox selection changed

}
