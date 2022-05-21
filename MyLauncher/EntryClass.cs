// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public class EntryClass : INotifyPropertyChanged
{
    #region Binding list
    public static BindingList<EntryClass> Entries { get; set; }
    #endregion Binding list

    #region Properties
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

    //Ignore the image as we don't want to save it in the JSON file
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
    private string title;
    private string filePathOrURI;
    private string arguments = string.Empty;
    private string iconSource = string.Empty;
    private ImageSource fileIcon;
    #endregion Private backing fields

    #region Handle property change
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change
}
