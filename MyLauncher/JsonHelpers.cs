// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;
/// <summary>
/// Class to help with JSON files
/// </summary>
internal static class JsonHelpers
{
    #region NLog Instance
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    #region Read the JSON file for the main list
    /// <summary>
    /// Read the JSON file and deserialize it into the ObservableCollection
    /// </summary>
    public static void ReadJson()
    {
        string jsonfile = GetMainListFile();

        if (!File.Exists(jsonfile))
        {
            CreateNewMainJson(jsonfile);
        }

        log.Debug($"Reading JSON file: {jsonfile}");
        try
        {
            string json = File.ReadAllText(jsonfile);
            Child.Children = JsonSerializer.Deserialize<ObservableCollection<Child>>(json);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            log.Error(ex, "File or Directory not found {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"File or Directory not found:\n\n{ex.Message}\n\nUnable to continue.",
                                "My Launcher Error",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error reading file: {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error reading file:\n\n{ex.Message}",
                                "My Launcher Error",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
        }

        if (Child.Children == null)
        {
            log.Error("File {0} is empty or is invalid", jsonfile);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"File {jsonfile} is empty or is invalid\n\nUnable to continue.",
                                "My Launcher Error",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
            Environment.Exit(2);
        }

        if (Child.Children.Count == 1)
        {
            log.Info($"Read {Child.Children.Count} entry from {jsonfile}");
        }
        else
        {
            log.Info($"Read {Child.Children.Count} entries from {jsonfile}");
        }
    }
    #endregion Read the JSON file for the main list

    #region Read the JSON file for the menu list
    /// <summary>
    /// Read the menu JSON file and deserialize it into the ObservableCollection
    /// </summary>
    public static void ReadMenuJson()
    {
        string jsonfile = GetMenuListFile();

        if (!File.Exists(jsonfile))
        {
            CreateNewMenuJson(jsonfile);
        }

        log.Debug($"Reading JSON file: {jsonfile}");
        try
        {
            string json = File.ReadAllText(jsonfile);
            MyMenuItem.MLMenuItems = JsonSerializer.Deserialize<ObservableCollection<MyMenuItem>>(json);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error reading file: {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error reading file:\n\n{ex.Message}",
                                "My Launcher Error",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
        }
        if (MyMenuItem.MLMenuItems.Count == 1)
        {
            log.Info($"Read {MyMenuItem.MLMenuItems.Count} entry from {jsonfile}");
        }
        else
        {
            log.Info($"Read {MyMenuItem.MLMenuItems.Count} entries from {jsonfile}");
        }
    }
    #endregion Read the JSON file for the menu list

    #region Save the main list JSON file
    /// <summary>
    ///  Serialize the ObservableCollection and write it to a JSON file
    /// </summary>
    public static void SaveMainJson()
    {
        List<Child> tempCollection = new();

        foreach (Child item in Child.Children)
        {
            if (!string.IsNullOrEmpty(item.Title))
            {
                Child ch = new()
                {
                    Title = item.Title.Trim(),
                    FilePathOrURI = item.FilePathOrURI.Trim('"').Trim(),
                    FileIcon = item.FileIcon,
                    Arguments = item.Arguments,
                    IconSource = item.IconSource.Trim(),
                    EntryType = item.EntryType,
                    ChildrenOfChild = item.ChildrenOfChild,
                    ItemID = item.ItemID,
                    PopupHeight = item.PopupHeight,
                    PopupLeft = item.PopupLeft,
                    PopupTop = item.PopupTop,
                    PopupWidth = item.PopupWidth,
                };
                tempCollection.Add(ch);
            }
        }
        JsonSerializerOptions opts = new() { WriteIndented = true };
        try
        {
            log.Info($"Saving JSON file: {GetMainListFile()}");
            string json = JsonSerializer.Serialize(tempCollection, opts);
            File.WriteAllText(GetMainListFile(), json);
            SnackbarMsg.QueueMessage("File Saved.", 2000);
            tempCollection.Clear();
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error saving file.");
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error saving file.\n{ex.Message}.",
                                "ERROR",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
        }
    }
    #endregion Save the main list JSON file

    #region Save the menu items JSON file
    /// <summary>
    ///  Serialize the ObservableCollection and write it to a JSON file
    /// </summary>
    public static void SaveMenuJson()
    {
        List<MyMenuItem> tempCollection = new();

        foreach (MyMenuItem item in MyMenuItem.MLMenuItems)
        {
            if (!string.IsNullOrEmpty(item.Title))
            {
                MyMenuItem mmi = new()
                {
                    Title = item.Title.Trim(),
                    FilePathOrURI = item.FilePathOrURI?.Trim('"').Trim(),
                    Arguments = item.Arguments,
                    ItemType = item.ItemType,
                    SubMenuItems = item.SubMenuItems,
                    ItemID = item.ItemID
                };
                tempCollection.Add(mmi);
            }
        }
        JsonSerializerOptions opts = new() { WriteIndented = true };
        try
        {
            log.Info($"Saving JSON file: {GetMenuListFile()}");
            string json = JsonSerializer.Serialize(tempCollection, opts);
            File.WriteAllText(GetMenuListFile(), json);
            SnackbarMsg.QueueMessage("File Saved.", 2000);
            tempCollection.Clear();
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error saving file.");
            SystemSounds.Exclamation.Play();
            MDCustMsgBox mbox = new($"Error saving file.\n{ex.Message}.",
                                "ERROR",
                                ButtonType.Ok,
                                true,
                                true,
                                null,
                                true);
            _ = mbox.ShowDialog();
        }
    }
    #endregion Save the menu items JSON file

    #region Create a backup for the main list file
    /// <summary>
    /// Creates a backup of the list file by copying the current file
    /// to a location of the user's choosing.
    /// </summary>
    public static void CreateBackupFile()
    {
        string tStamp = string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now);
        SaveFileDialog dlgSave = new()
        {
            Title = "Choose backup location",
            CheckPathExists = true,
            CheckFileExists = false,
            OverwritePrompt = true,
            FileName = $"MyLauncher_{tStamp}_backup.json",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*"
        };
        if (dlgSave.ShowDialog() == true)
        {
            try
            {
                File.Copy(GetMainListFile(), dlgSave.FileName, true);
                log.Info($"List backed up to {dlgSave.FileName}");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Backup failed.");
                SystemSounds.Exclamation.Play();
                MDCustMsgBox mbox = new($"Backup failed.\n{ex.Message}.",
                                    "ERROR",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    null,
                                    true);
                _ = mbox.ShowDialog();
            }
        }
    }
    #endregion Create a backup for the list file

    #region Create a backup for the menu file
    /// <summary>
    /// Creates a backup of the menu file by copying the current file
    /// to a location of the user's choosing.
    /// </summary>
    public static void CreateMenuBackup()
    {
        string tStamp = string.Format("{0:yyyyMMdd_HHmm}", DateTime.Now);
        SaveFileDialog dlgSave = new()
        {
            Title = "Choose backup location",
            CheckPathExists = true,
            CheckFileExists = false,
            OverwritePrompt = true,
            FileName = $"MyLauncher_{tStamp}_menu_backup.json",
            Filter = "JSON (*.json)|*.json|All files (*.*)|*.*"
        };
        if (dlgSave.ShowDialog() == true)
        {
            try
            {
                File.Copy(GetMenuListFile(), dlgSave.FileName, true);
                log.Info($"List backed up to {dlgSave.FileName}");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Backup failed.");
                SystemSounds.Exclamation.Play();
                MDCustMsgBox mbox = new($"Backup failed.\n{ex.Message}.",
                                    "ERROR",
                                    ButtonType.Ok,
                                    true,
                                    true,
                                    null,
                                    true);
                _ = mbox.ShowDialog();
            }
        }
    }
    #endregion Create a backup for the menu file

    #region Create starter main JSON file
    /// <summary>
    /// Creates a new JSON file containing entries for the Windows calculator
    /// </summary>
    /// <param name="file">Name of the main list file</param>
    private static void CreateNewMainJson(string file)
    {
        Guid guid = Guid.NewGuid();
        string thisguid = guid.ToString();
        StringBuilder sb = new();
        _ = sb.AppendLine("[")
              .AppendLine("  {")
              .AppendLine("    \"Title\": \"Calculator\",")
              .Append("    \"ItemID\": \"")
              .Append(thisguid)
              .AppendLine("\",")
              .AppendLine("    \"EntryType\": 0,")
              .AppendLine("    \"FilePathOrURI\": \"calc.exe\",")
              .AppendLine("    \"Arguments\": \"\",")
              .AppendLine("    \"IconSource\": \"\",")
              .AppendLine("    \"Children\": null")
              .AppendLine("  }")
              .AppendLine("]");
        File.WriteAllText(file, sb.ToString());
        log.Debug($"Creating new JSON file for main list - {file}");
    }
    #endregion Create starter main JSON file

    #region Create starter menu JSON file
    /// <summary>
    /// Creates a new JSON file containing menu item for the Windows calculator
    /// </summary>
    /// <param name="file">Name of the main list file</param>
    private static void CreateNewMenuJson(string file)
    {
        Guid guid = Guid.NewGuid();
        string thisguid = guid.ToString();
        StringBuilder sb = new();
        _ = sb.AppendLine("[")
              .AppendLine("  {")
              .AppendLine("    \"Title\": \"Calculator\",")
              .AppendLine("    \"ItemType\": 1,")
              .AppendLine("    \"FilePathOrURI\": \"calc.exe\",")
              .AppendLine("    \"Arguments\": \"\",")
              .Append("    \"ItemID\": \"")
              .Append(thisguid)
              .AppendLine("\",")
              .AppendLine("    \"MenuItems\": null")
              .AppendLine("  }")
              .AppendLine("]");
        File.WriteAllText(file, sb.ToString());
        log.Debug($"Creating new JSON file for menu items - {file}");
    }
    #endregion Create starter menu JSON file

    #region Get the name of the main list JSON file
    /// <summary>
    /// Gets the filename of the JSON file containing the main list
    /// </summary>
    /// <returns>A string containing the filename</returns>
    public static string GetMainListFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncherItems.json");
    }
    #endregion Get the name of the main list JSON file

    #region Get the name of the menu items JSON file
    /// <summary>
    /// Gets the filename of the JSON file containing the menu items
    /// </summary>
    /// <returns>A string containing the filename</returns>
    public static string GetMenuListFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyMenuItems.json");
    }
    #endregion Get the name of the menu items JSON file
}