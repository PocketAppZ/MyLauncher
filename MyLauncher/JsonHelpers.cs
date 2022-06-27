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
            CreateNewJson(jsonfile);
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
            _ = new MDCustMsgBox($"File or Directory not found:\n\n{ex.Message}\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error reading file: {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error reading file:\n\n{ex.Message}",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
        }

        if (Child.Children == null)
        {
            log.Error("File {0} is empty or is invalid", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"File {jsonfile} is empty or is invalid\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
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

    #region Save the JSON file
    /// <summary>
    ///  Serialize the ObservableCollection and write it to a JSON file
    /// </summary>
    public static void SaveJson()
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
            _ = new MDCustMsgBox($"Error saving file.\n{ex.Message}.",
                "ERROR", ButtonType.Ok).ShowDialog();
        }
    }
    #endregion Save the JSON file

    #region Create a backup for the list file
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
                _ = new MDCustMsgBox($"Backup failed.\n{ex.Message}.",
                    "ERROR", ButtonType.Ok).ShowDialog();
                log.Error(ex, "Backup failed.");
            }
        }
    }
    #endregion Create a backup for the list file

    #region Create starter JSON file
    /// <summary>
    /// Creates a new JSON file containing entries for the Windows calculator and notepad
    /// </summary>
    /// <param name="file">Name of the main list file</param>
    private static void CreateNewJson(string file)
    {
        StringBuilder sb = new();
        _ = sb.AppendLine("[")
            .AppendLine("  {")
            .AppendLine("    \"Arguments\": \"\",")
            .AppendLine("    \"EntryType\": 0,")
            .AppendLine("    \"FilePathOrURI\": \"calc.exe\",")
            .AppendLine("    \"IconSource\": \"\",")
            .AppendLine("    \"Title\": \"Calculator\" ")
            .AppendLine("  },")
            .AppendLine("  {")
            .AppendLine("    \"Arguments\": \"\",")
            .AppendLine("    \"EntryType\": 0,")
            .AppendLine("    \"FilePathOrURI\": \"notepad.exe\",")
            .AppendLine("    \"IconSource\": \"\",")
            .AppendLine("    \"Title\": \"Notepad\" ")
            .AppendLine("  }")
            .AppendLine("]");
        File.WriteAllText(file, sb.ToString());
        log.Debug($"Creating new JSON file with one entry - {file}");
    }
    #endregion Create starter JSON file

    #region Get the name of the JSON file
    /// <summary>
    /// Gets the filename of the JSON file containing the main list
    /// </summary>
    /// <returns>A string containing the filename</returns>
    public static string GetMainListFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncher.json");
    }
    #endregion Get the name of the JSON file
}