// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace MyLauncher;

/// <summary>
/// Class for all of the objects in the menu list. Also includes the drag and drop handlers.
/// </summary>
public class MyMenuItem : INotifyPropertyChanged, IDropTarget
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
    /// Working Directory for the item
    /// </summary>
    public string WorkingDir
    {
        get => workingDir;
        set
        {
            if (arguments != null)
            {
                workingDir = value;
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
    private string workingDir = string.Empty;
    #endregion Private backing fields

    #region Handle property change
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change

    #region Drag & Drop handlers. Who knew they were this much fun?
    /// <summary>
    /// Drag handler. If drop target is not a Submenu, disallow drop.
    /// </summary>
    /// <param name="dropInfo"></param>
    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.TargetItem is MyMenuItem dragTarget)
        {
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                if (dragTarget.ItemType is MenuItemType.MenuItem or MenuItemType.Separator or MenuItemType.SectionHead)
                {
                    return;
                }
            }
            DragDrop.DefaultDropHandler.DragOver(dropInfo);
        }
    }

    /// <summary>
    /// Drop handler. If the drop target is a Submenu, expand the TreeView item. Otherwise use defaults.
    /// </summary>
    /// <param name="dropInfo"></param>
    void IDropTarget.Drop(IDropInfo dropInfo)
    {
        if (dropInfo.TargetItem is MyMenuItem dropItem)
        {
            if (dropItem.ItemType == MenuItemType.SubMenu)
            {
                DragDrop.DefaultDropHandler.Drop(dropInfo);
                TreeViewItem tvi = dropInfo.VisualTargetItem as TreeViewItem;
                tvi.IsExpanded = true;
                tvi.BringIntoView();
            }
            else if ((dropItem.ItemType == MenuItemType.MenuItem || dropItem.ItemType == MenuItemType.Separator)
                && !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }
    }
    #endregion Drag & Drop handlers. Who knew that they were this much fun?
}
