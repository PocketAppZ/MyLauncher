// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using MyLauncher;

namespace TLDataToMLData;

public class MyMenuItem : INotifyPropertyChanged
{
    #region ObservableCollection
    /// <summary>
    /// ObservableCollection for the menu items.
    /// </summary>
    [JsonPropertyName("MenuItems")]
    public static ObservableCollection<MyMenuItem> MLMenuItems { get; set; }
    #endregion ObservableCollection

    #region Properties
    /// <summary>
    /// Title for the entry
    /// </summary>
    public string Title
    {
        get => title;
        set
        {
            if (value != null)
            {
                title = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Menu item type
    /// </summary>
    public MenuItemType ItemType
    {
        get => itemType;
        set
        {
            itemType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Path to file folder or URL
    /// </summary>
    public string FilePathOrURI
    {
        get => filePathOrURI;
        set
        {
            if (value != null)
            {
                filePathOrURI = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Command line arguments for FilePathOrURI
    /// </summary>
    public string Arguments
    {
        get => arguments;
        set
        {
            if (arguments != null)
            {
                arguments = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Unique Identifier for the item
    /// </summary>
    public string ItemID
    {
        get => itemID;
        set
        {
            itemID = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ObservableCollection for the submenu items.
    /// </summary>
    [JsonPropertyName("MenuItems")]
    public ObservableCollection<MyMenuItem> SubMenuItems { get; set; }

    /// <summary>
    /// This property is part of a workaround to select an item in the TreeView
    /// It is also ignored since we don't want it in the JSON file
    /// </summary>
    [JsonIgnore]
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; OnPropertyChanged(); }
    }
    #endregion Properties

    #region Private backing fields
    private string arguments = string.Empty;
    private string filePathOrURI;
    private bool isSelected;
    private string itemID;
    private MenuItemType itemType = MenuItemType.MenuItem;
    private string title;
    #endregion Private backing fields

    #region Handle property change
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change

}
