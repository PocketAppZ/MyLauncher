namespace MyLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for Manitenance.xaml
    /// </summary>
    public partial class Maintenance : UserControl
    {
        #region NLog
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        #endregion NLog

        public static bool EntriesChanged { get; set; }
        public static object PrevEntry { get; set; }

        public Maintenance()
        {
            InitializeComponent();

            LoadListBox();

            EntryClass.Entries.ListChanged += Entries_ListChanged;
        }

        #region List changed event
        private void Entries_ListChanged(object sender, ListChangedEventArgs e)
        {
            EntriesChanged = true;
            btnDiscard.IsEnabled = true;
        }
        #endregion List changed event

        #region Load the list box
        private void LoadListBox()
        {
            if (EntryClass.Entries.Count > 0)
            {
                listbox1.ItemsSource = EntryClass.Entries;
                listbox1.SelectedItem = EntryClass.Entries.First();
                _ = listbox1.Focus();
            }
        }
        #endregion Load the list box

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
                    _ = new MDCustMsgBox("Please update or delete the untitled entry before adding another new entry.",
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
            EntryClass newitem = new() { Title = string.Empty };
            EntryClass.Entries.Add(newitem);
            listbox1.SelectedIndex = listbox1.Items.Count - 1;
            listbox1.ScrollIntoView(listbox1.SelectedItem);
            _ = tb1.Focus();
            SnackbarMsg.ClearAndQueueMessage("New \"untitled\" item was created.", 5000);
        }
        #endregion Add New item

        #region Delete an item
        private void DeleteItem()
        {
            if (listbox1.SelectedItem != null)
            {
                var item = (listbox1.SelectedItem as EntryClass)?.Title;
                _ = EntryClass.Entries.Remove((EntryClass)listbox1.SelectedItem);
                SnackbarMsg.ClearAndQueueMessage($"Deleted \"{item}\"", 1000);
                listbox1.SelectedItem = EntryClass.Entries.First();
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
                if (!string.IsNullOrEmpty(item.Title) && (!string.IsNullOrEmpty(item.FilePathOrURI)))
                {
                    EntryClass ec = new()
                    {
                        Title = item.Title.Trim(),
                        FilePathOrURI = item.FilePathOrURI.Trim(),
                        FileIcon = item.FileIcon,
                        Arguments = item.Arguments,
                        IconSource = item.IconSource.Trim(),
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

        #region File picker button (for Path)
        private void BtnFilePicker_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new()
            {
                Title = "Choose a File",
                Multiselect = false,
                CheckFileExists = false,
                CheckPathExists = true,
            };
            if (!string.IsNullOrEmpty(tb2.Text) && File.Exists(tb2.Text.Trim()))
            {
                dlgOpen.FileName = tb2.Text;
            }
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                tb2.Text = dlgOpen.FileName;
                EntryClass entry = (EntryClass)listbox1.SelectedItem;
                entry.FilePathOrURI = tb2.Text;
            }
        }
        #endregion File picker button (for Path)

        #region Button events
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckForUntitled())
            {
                NewItem();
            }
        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow as MainWindow)?.ReadJson();
            (Application.Current.MainWindow as MainWindow)?.ResetListBox();
            LoadListBox();
            listbox1.Items.Refresh();
            EntriesChanged = false;
            btnDiscard.IsEnabled = false;
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

        private void BtnSpecial_Click(object sender, RoutedEventArgs e)
        {
            EntryClass newitem = new();
            Button btn = sender as Button;
            switch (btn.Content.ToString())
            {
                case "Calculator":
                    newitem.Title = "Calculator";
                    newitem.FilePathOrURI = "calculator:";
                    newitem.IconSource = "calc.png";
                    break;
                case "Calendar":
                    newitem.Title = "Calendar";
                    newitem.FilePathOrURI = "outlookcal:";
                    newitem.IconSource = "calendar.png";
                    break;
                case "Mail":
                    newitem.Title = "Mail";
                    newitem.FilePathOrURI = "outlookmail:";
                    newitem.IconSource = "mail.png";
                    break;
                case "Solitaire":
                    newitem.Title = "Solitaire Collection";
                    newitem.FilePathOrURI = "xboxliveapp-1297287741:";
                    newitem.IconSource = "cards.png";
                    break;
                case "Weather":
                    newitem.Title = "Weather";
                    newitem.FilePathOrURI = "bingweather:";
                    newitem.IconSource = "weather.png";
                    break;
            }
            if (tb1.Text != string.Empty)
            {
                EntryClass.Entries.Add(newitem);
                listbox1.SelectedIndex = listbox1.Items.Count - 1;
                EntriesChanged = true;
            }
        }

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
                    tb3.Text = Path.GetFileName(dlgOpen.FileName);
                    EntryClass entry = (EntryClass)listbox1.SelectedItem;
                    entry.IconSource = tb3.Text;
                }
                else
                {
                    tb3.Text = dlgOpen.FileName;
                    EntryClass entry = (EntryClass)listbox1.SelectedItem;
                    entry.IconSource = tb3.Text;
                }
            }
        }

        private void Listbox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listbox1.SelectedItem == null)
            {
                listbox1.SelectedItem = PrevEntry;
                return;
            }
            PrevEntry = listbox1.SelectedItem;
        }
    }
}
