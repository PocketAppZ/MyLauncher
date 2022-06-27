// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;
/// <summary>
/// Class for all of the objects in the list. Also includes the drag and drop handlers.
/// </summary>
public class Child : INotifyPropertyChanged, IDropTarget
{
    #region ObservableCollection for Child
    /// <summary>
    /// Observable collection containing the list entries
    /// </summary>
    [JsonPropertyName("Children")]
    public static ObservableCollection<Child> Children { get; set; }
    #endregion ObservableCollection for Child

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

    private string itemID;

    public string ItemID
    {
        get { return itemID; }
        set { itemID = value; OnPropertyChanged(); }
    }


    /// <summary>
    /// Entry type, 0 = normal item, 1 = pop-up
    /// </summary>
    public ListEntryType EntryType
    {
        get => entryType;
        set
        {
            entryType = value;
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
    /// Path to image file for icons
    /// </summary>
    public string IconSource
    {
        get => iconSource;
        set
        {
            if (value != null)
            {
                iconSource = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Pop-up window location
    /// </summary>
    public double PopupTop { get; set; }

    /// <summary>
    /// Pop-up window location
    /// </summary>
    public double PopupLeft { get; set; }

    /// <summary>
    /// Pop-up window height
    /// </summary>
    public double PopupHeight { get; set; }

    /// <summary>
    /// Pop-up window width
    /// </summary>
    public double PopupWidth { get; set; }

    /// <summary>
    /// Observable collection containing the entries
    /// </summary>
    [JsonPropertyName("Children")]
    public ObservableCollection<Child> ChildrenOfChild { get; set; }

    /// <summary>
    /// Ignore the image as we don't want to save it in the JSON file
    /// </summary>
    [JsonIgnore]
    public ImageSource FileIcon
    {
        get => fileIcon;
        set
        {
            if (value != null)
            {
                fileIcon = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion Properties

    #region Private backing fields
    private string arguments = string.Empty;
    private ListEntryType entryType = ListEntryType.Normal;
    private ImageSource fileIcon;
    private string filePathOrURI;
    private string iconSource = string.Empty;
    private string title;
    #endregion Private backing fields

    #region Handle property change
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change

    #region Drag & Drop handlers
    /// <summary>
    /// Drag handler. If drop target is not a pop-up disallow drop.
    /// </summary>
    /// <param name="dropInfo"></param>
    void IDropTarget.DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.TargetItem is Child dragTarget
            && dragTarget.EntryType == (int)ListEntryType.Normal
            && dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
        {
            return;
        }
        GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
    }

    /// <summary>
    /// Drop handler. If the drop target is a pop-up expand the TreeView item. Otherwise use defaults.
    /// </summary>
    /// <param name="dropInfo"></param>
    void IDropTarget.Drop(IDropInfo dropInfo)
    {
        if (dropInfo.TargetItem is Child dropItem)
        {
            if (dropItem.EntryType == ListEntryType.Popup)
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
                TreeViewItem tvi = dropInfo.VisualTargetItem as TreeViewItem;
                tvi.IsExpanded = true;
            }
            else if (dropItem.EntryType == ListEntryType.Normal
                && !dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter))
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
            }
        }
    }
    #endregion Drag & Drop handlers
}
