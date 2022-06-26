// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
{
    #region Properties

    public bool AllowRightButton
    {
        get => allowRightButton;
        set
        {
            allowRightButton = value;
            OnPropertyChanged();
        }
    }

    public int BorderWidth
    {
        get { return borderWidth; }
        set
        {
            borderWidth = value;
            OnPropertyChanged();
        }
    }

    public int DarkMode
    {
        get => darkmode;
        set
        {
            darkmode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeDebug
    {
        get => includeDebug;
        set
        {
            includeDebug = value;
            OnPropertyChanged();
        }
    }

    public bool KeepOnTop
    {
        get => keepOnTop;
        set
        {
            keepOnTop = value;
            OnPropertyChanged();
        }
    }

    public int ListBoxFontWeight
    {
        get { return listBoxFontWeight; }
        set
        {
            listBoxFontWeight = value;
            OnPropertyChanged();
        }
    }

    public int ListBoxSpacing
    {
        get => listBoxSpacing;
        set
        {
            listBoxSpacing = value;
            OnPropertyChanged();
        }
    }

    public bool MaintFistRun { get; set; } = true;

    public double MaintWindowHeight { get; set; } = 540;

    public double MaintWindowLeft
    {
        get
        {
            if (maintWindowLeft < 0)
            {
                maintWindowLeft = 0;
            }
            return maintWindowLeft;
        }
        set => maintWindowLeft = value;
    }

    public double MaintWindowTop
    {
        get
        {
            if (maintWindowTop < 0)
            {
                maintWindowTop = 0;
            }
            return maintWindowTop;
        }
        set => maintWindowTop = value;
    }


    public bool MainWindowMinimizeOnLaunch
    {
        get { return mainWindowMinimizeOnLaunch; }
        set { mainWindowMinimizeOnLaunch = value; OnPropertyChanged(); }
    }


    public double MaintWindowWidth { get; set; } = 850;

    public bool MinimizeToTray
    {
        get => minimizeToTray;
        set
        {
            minimizeToTray = value;
            OnPropertyChanged();
        }
    }

    public bool PlaySound
    {
        get => playSound;
        set
        {
            playSound = value;
            OnPropertyChanged();
        }
    }

    public bool PopupCloseAfterLaunch
    {
        get => popupCloseAfterLaunch;
        set
        {
            popupCloseAfterLaunch = value;
            OnPropertyChanged();
        }
    }

    public int PrimaryColor
    {
        get => primaryColor;
        set
        {
            primaryColor = value;
            OnPropertyChanged();
        }
    }

    public int SecondaryColor
    {
        get => secondaryColor;
        set
        {
            secondaryColor = value;
            OnPropertyChanged();
        }
    }

    public double SettingsWindowHeight
    {
        get
        {
            if (settingsWindowHeight < 100)
            {
                settingsWindowHeight = 100;
            }
            return settingsWindowHeight;
        }
        set => settingsWindowHeight = value;
    }

    public double SettingsWindowLeft
    {
        get
        {
            if (settingsWindowLeft < 0)
            {
                settingsWindowLeft = 0;
            }
            return settingsWindowLeft;
        }
        set => settingsWindowLeft = value;
    }

    public double SettingsWindowTop
    {
        get
        {
            if (settingsWindowTop < 0)
            {
                settingsWindowTop = 0;
            }
            return settingsWindowTop;
        }
        set => settingsWindowTop = value;
    }

    public bool ShowExitButton
    {
        get { return showExitButton; }
        set
        {
            showExitButton = value;
            OnPropertyChanged();
        }
    }

    public bool ShowFileIcons
    {
        get => showFileIcons;
        set
        {
            showFileIcons = value;
            OnPropertyChanged();
        }
    }

    public bool StartMinimized
    {
        get => startMinimized;
        set
        {
            startMinimized = value;
            OnPropertyChanged();
        }
    }

    public bool StartWithWindows
    {
        get => startWithWindows;
        set
        {
            startWithWindows = value;
            OnPropertyChanged();
        }
    }

    public string TitleText
    {
        get => titleText;
        set
        {
            titleText = value;
            OnPropertyChanged();
        }
    }

    public int UISize
    {
        get => uiSize;
        set
        {
            uiSize = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get
        {
            if (windowHeight < 100)
            {
                windowHeight = 100;
            }
            return windowHeight;
        }
        set => windowHeight = value;
    }

    public double WindowLeft
    {
        get
        {
            if (windowLeft < 0)
            {
                windowLeft = 0;
            }
            return windowLeft;
        }
        set => windowLeft = value;
    }

    public double WindowTop
    {
        get
        {
            if (windowTop < 0)
            {
                windowTop = 0;
            }
            return windowTop;
        }
        set => windowTop = value;
    }

    public double WindowWidth
    {
        get
        {
            if (windowWidth < 100)
            {
                windowWidth = 100;
            }
            return windowWidth;
        }
        set => windowWidth = value;
    }
    #endregion Properties

    #region Private backing fields
    private bool allowRightButton = true;
    private int borderWidth = 2;
    private int darkmode = (int)ThemeType.Light;
    private bool includeDebug = false;
    private bool keepOnTop = false;
    private int listBoxFontWeight = (int)Weight.Regular;
    private int listBoxSpacing = (int)Spacing.Comfortable;
    private bool mainWindowMinimizeOnLaunch = false;
    private double maintWindowLeft = 100;
    private double maintWindowTop = 100;
    private bool minimizeToTray = false;
    private bool playSound = true;
    private bool popupCloseAfterLaunch = true;
    private int primaryColor = (int)AccentColor.Blue;
    private int secondaryColor = (int)AccentColor.Red;
    private double settingsWindowHeight = 660;
    private double settingsWindowLeft = 100;
    private double settingsWindowTop = 100;
    private bool showExitButton = true;
    private bool showFileIcons = true;
    private bool startMinimized = false;
    private bool startWithWindows = false;
    private string titleText = "Click on any App Below to Open it";
    private int uiSize = (int)MySize.Default;
    private double windowHeight = 550;
    private double windowLeft = 100;
    private double windowTop = 100;
    private double windowWidth = 875;
    #endregion Private backing fields

    #region Handle property change event
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change event
}
