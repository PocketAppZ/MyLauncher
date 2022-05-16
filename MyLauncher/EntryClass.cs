// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

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

    [JsonIgnore]
    public ImageSource FileIcon { get; set; }
    #endregion Properties

    #region Private backing fields
    private string title;
    private string filePathOrURI;
    private string iconSource = string.Empty;
    #endregion Private backing fields

    #region Handle property change
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change
}
