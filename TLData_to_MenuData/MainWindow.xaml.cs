// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.Json;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using MyLauncher;
#endregion Using directives

namespace TLDataToMLData;

public partial class MainWindow : Window
{
    #region Private fields
    private MenuList XmlData;
    private string xmlMenuFile;
    private string jsonDataFile;
    private readonly List<MyMenuItem> menuItems = new();
    private readonly List<Child> listItems = new();
    private MyMenuItem currentSubMenu;
    private Child currentPopup;
    #endregion Private fields

    public MainWindow()
    {
        InitializeComponent();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        tbTrayLauncherFile.Text = Environment.ExpandEnvironmentVariables("%localappdata%\\Programs\\T_K\\TrayLauncher\\MenuItems.xml");
        _ = lbxOutput.Items.Add("1. Select either the \"Menu Items\" or \"List Items\" conversion type. The My Launcher file name will be filled for you.");
        _ = lbxOutput.Items.Add("2. The Tray Launcher file name is set to the default location.");
        _ = lbxOutput.Items.Add("3. Change either file name if needed.");
        _ = lbxOutput.Items.Add("4. Check the box at the bottom to save the output to a text file.");
        _ = lbxOutput.Items.Add("5. Click the Convert button when ready.");
        _ = lbxOutput.Items.Add("6. Use the Import function in the appropriate Maintenance window in My Launcher to import the converted file.");
    }

    #region Button click events
    private void RbMenu_Checked(object sender, RoutedEventArgs e)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        tbMyLauncherFile.Text = Path.Combine(desktop, "Converted_MenuItems.json");
    }

    private void RbList_Checked(object sender, RoutedEventArgs e)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        tbMyLauncherFile.Text = Path.Combine(desktop, "Converted_LauncherItems.json");
    }

    private void BtnConvertList_Click(object sender, RoutedEventArgs e)
    {
        lbxOutput.Items.Clear();
        if (rbList.IsChecked == true)
        {
            ReadTrayLauncher();
            ConvertToMLList();
        }
        else if (rbMenu.IsChecked == true)
        {
            ReadTrayLauncher();
            ConvertToMLMenu();
        }
        else
        {
            _ = lbxOutput.Items.Add("⚠️ Please select conversion type!");
            SystemSounds.Exclamation.Play();
        }
    }

    private void BtnReadMe_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(@".\Conversion.txt");
    }

    private void BtnTLFilePicker_Click(object sender, RoutedEventArgs e)
    {
        string tlPath = Environment.ExpandEnvironmentVariables("%localappdata%\\Programs\\T_K\\TrayLauncher");
        OpenFileDialog dlgOpen = new()
        {
            Title = "Browse for a File",
            Multiselect = false,
            CheckFileExists = true,
            FileName = "MenuItems.xml",
            InitialDirectory = tlPath,
            Filter = "Tray Launcher data (*.xml)|*.xml|All files (*.*)|*.*"
        };
        bool? result = dlgOpen.ShowDialog();
        if (result == true)
        {
            tbTrayLauncherFile.Text = dlgOpen.FileName;
        }
    }

    private void BtnMLFilePicker_Click(object sender, RoutedEventArgs e)
    {
        string mlPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        OpenFileDialog dlgOpen = new()
        {
            Title = "Browse for a File",
            Multiselect = false,
            CheckFileExists = false,
            InitialDirectory = mlPath,
            Filter = "My Launcher data (*.json)|*.json|All files (*.*)|*.*"
        };
        bool? result = dlgOpen.ShowDialog();
        if (result == true)
        {
            tbMyLauncherFile.Text = dlgOpen.FileName;
        }
    }
    #endregion Button click events

    #region Read TL XML file
    private void ReadTrayLauncher()
    {
        xmlMenuFile = tbTrayLauncherFile.Text.Trim('"');
        _ = lbxOutput.Items.Add($"Opening file: \"{xmlMenuFile}\"");
        try
        {
            XmlSerializer deserializer = new(typeof(MenuList));
            TextReader reader = new StreamReader(xmlMenuFile);
            XmlData = (MenuList)deserializer.Deserialize(reader);
            reader.Close();
            _ = lbxOutput.Items.Add($"✔️ Read {XmlData.menuList.Count} Tray Launcher menu items");
        }
        catch (Exception ex)
        {
            _ = lbxOutput.Items.Add($"⚠️ Error reading \"{xmlMenuFile}\"");
            _ = lbxOutput.Items.Add($"⚠️ {ex.Message}");
            SystemSounds.Exclamation.Play();
        }
    }
    #endregion Read TL XML file

    #region Convert TL data to My Launcher list data
    private void ConvertToMLList()
    {
        if (XmlData == null)
        {
            _ = lbxOutput.Items.Add("⚠️ Error converting data");
            SystemSounds.Exclamation.Play();
            return;
        }
        int skip = 0;
        int keep = 0;
        listItems.Clear();
        foreach (TLMenuItem item in XmlData.menuList)
        {
            Guid guid = Guid.NewGuid();
            Child child = new();

            // Skip Section Headers. No analogous property in ML.
            if (item.ItemType == "SH")
            {
                _ = lbxOutput.Items.Add($"➖ Skipping Section Header \"{item.Header}\" ");
                skip++;
            }
            // Separator
            else if (item.ItemType == "SEP")
            {
                _ = lbxOutput.Items.Add("➖ Skipping Separator ");
                skip++;
            }
            // SubMenu
            else if (item.ItemType == "SM")
            {
                child.Title = item.Header;
                child.FilePathOrURI = "";
                child.ItemID = guid.ToString();
                child.EntryType = ListEntryType.Popup;
                child.IconSource = "menu.png";
                child.ChildrenOfChild = new ObservableCollection<Child>();
                listItems.Add(child);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Pop-up");
                currentPopup = child;
                keep++;
            }
            // SubMenu item
            else if (item.ItemType == "SMI")
            {
                child.Title = item.Header;
                child.FilePathOrURI = item.AppPath.Trim('"');
                child.Arguments = item.Arguments;
                child.ItemID = guid.ToString();
                child.EntryType = ListEntryType.Normal;
                currentPopup.ChildrenOfChild.Add(child);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Normal item under {currentPopup.Title}");
                keep++;
            }
            // Menu item
            else
            {
                child.Title = item.Header;
                child.FilePathOrURI = item.AppPath.Trim('"');
                child.Arguments = item.Arguments;
                child.ItemID = guid.ToString();
                child.EntryType = ListEntryType.Normal;
                listItems.Add(child);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Normal item");
                keep++;
            }
        }
        _ = lbxOutput.Items.Add($"👉 Skipped {skip} Section Header and/or Separator entries");
        _ = lbxOutput.Items.Add($"⚡ Converted {keep} My Launcher list entries");
        WriteJson(listItems);
    }
    #endregion Convert TL data to My Launcher list data

    #region Convert TL data to My Launcher menu data
    private void ConvertToMLMenu()
    {
        if (XmlData == null)
        {
            _ = lbxOutput.Items.Add("⚠️ Error converting data");
            SystemSounds.Exclamation.Play();
            return;
        }
        int skip = 0;
        int keep = 0;
        menuItems.Clear();
        foreach (TLMenuItem item in XmlData.menuList)
        {
            Guid guid = Guid.NewGuid();
            MyMenuItem menuItem = new();

            // Skip Section Headers. No analogous property in ML.
            if (item.ItemType == "SH")
            {
                _ = lbxOutput.Items.Add($"➖ Skipping Section Header \"{item.Header}\" ");
                skip++;
            }
            // Separator
            else if (item.ItemType == "SEP")
            {
                menuItem.Title = "*Separator*";
                menuItem.FilePathOrURI = "";
                menuItem.ItemID = guid.ToString();
                menuItem.ItemType = MenuItemType.Separator;
                menuItems.Add(menuItem);
                _ = lbxOutput.Items.Add("➕ Adding Separator ");
            }
            // SubMenu
            else if (item.ItemType == "SM")
            {
                menuItem.Title = item.Header;
                menuItem.FilePathOrURI = "";
                menuItem.ItemID = guid.ToString();
                menuItem.ItemType = MenuItemType.SubMenu;
                menuItem.SubMenuItems = new ObservableCollection<MyMenuItem>();
                menuItems.Add(menuItem);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Submenu");
                currentSubMenu = menuItem;
                keep++;
            }
            // SubMenu item
            else if (item.ItemType == "SMI")
            {
                menuItem.Title = item.Header;
                menuItem.FilePathOrURI = item.AppPath.Trim('"');
                menuItem.Arguments = item.Arguments;
                menuItem.ItemID = guid.ToString();
                menuItem.ItemType = MenuItemType.MenuItem;
                currentSubMenu.SubMenuItems.Add(menuItem);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Normal item under {currentSubMenu.Title}");
                keep++;
            }
            // Menu item
            else
            {
                menuItem.Title = item.Header;
                menuItem.FilePathOrURI = item.AppPath.Trim('"');
                menuItem.Arguments = item.Arguments;
                menuItem.ItemID = guid.ToString();
                menuItem.ItemType = MenuItemType.MenuItem;
                menuItems.Add(menuItem);
                _ = lbxOutput.Items.Add($"➕ Adding \"{item.Header}\" as Normal item");
                keep++;
            }
        }
        _ = lbxOutput.Items.Add($"👉 Skipped {skip} Section Header entries");
        _ = lbxOutput.Items.Add($"⚡ Converted {keep} My Launcher menu entries");
        WriteJson(menuItems);
    }
    #endregion Convert TL data to My Launcher Menu data

    #region Write JSON
    private void WriteJson(object list)
    {
        try
        {
            jsonDataFile = tbMyLauncherFile.Text.Trim('"');
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(list, opts);
            File.WriteAllText(jsonDataFile, json);
            _ = lbxOutput.Items.Add($"✔️ Saving JSON file: \"{jsonDataFile}\"");
        }
        catch (Exception ex)
        {
            _ = lbxOutput.Items.Add($"⚠️ Error saving JSON file: \"{jsonDataFile}\"");
            _ = lbxOutput.Items.Add($"⚠️ {ex.Message}");
            SystemSounds.Exclamation.Play();
        }
        lbxOutput.SelectedIndex = lbxOutput.Items.Count - 1;
        lbxOutput.ScrollIntoView(lbxOutput.SelectedItem);
        SaveOutput(jsonDataFile);
    }
    #endregion Write JSON

    #region Save listbox to text file
    private void SaveOutput(string jsonfile)
    {
        if (cbSaveLog.IsChecked == true)
        {
            string convtype;
            if (jsonfile.Contains("Menu"))
            {
                convtype = "Convert_to_Menu_Log.txt";
            }
            else
            {
                convtype = "Convert_to_List_Log.txt";
            }
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.AppendAllLines(Path.Combine(desktop, convtype), lbxOutput.Items.Cast<string>());
        }
    }
    #endregion Save listbox to text file

    #region Unhandled Exception Handler
    /// <summary>
    /// Handles any exceptions that weren't caught by a try-catch statement
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        _ = MessageBox.Show($"An error has occurred.\n {e.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
    }
    #endregion Unhandled Exception Handler
}
