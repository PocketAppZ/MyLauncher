// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class App : Application
{
    public Mutex Mutex;
    public static string[] Args { get; set; }
    public App()
    {
        SingleInstanceCheck();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // Show splash screen unless command line argument "NoSplash" is specified
        Args = e.Args;
        if (!Args.Any(s => s.Contains("NoSplash", StringComparison.OrdinalIgnoreCase)))
        {
            const string splashPath = @"\Images\ML.png";
            SplashScreen splashScreen = new(splashPath);
            splashScreen.Show(true);
        }

        base.OnStartup(e);
    }

    public void SingleInstanceCheck()
    {
        Mutex = new Mutex(true, "MyLauncher", out bool isOnlyInstance);
        if (!isOnlyInstance)
        {
            // get our process info and then loop the other processes
            Process curProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(curProcess.ProcessName))
            {
                // if the process id is not the same and the executable has the same path
                if (!process.Id.Equals(curProcess.Id)
                    && process.MainModule.FileName.Equals(curProcess.MainModule.FileName))
                {
                    // get the handle of the other process
                    IntPtr windowHandle = process.MainWindowHandle;

                    // if the app is minimized restore it
                    if (NativeMethods.IsIconic(windowHandle))
                    {
                        _ = NativeMethods.ShowWindow(windowHandle,
                            NativeMethods.ShowWindowCommand.Restore);
                    }

                    // move the app to the foreground
                    _ = NativeMethods.SetForegroundWindow(windowHandle);
                }
            }
            Environment.Exit(0);
        }
    }

    private void ListBoxItem_MouseClick(object sender, MouseButtonEventArgs e)
    {
        (Current.MainWindow as MainWindow)?.ListBoxItem_MouseClick(sender, e);
    }
}
