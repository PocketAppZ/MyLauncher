﻿// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

/// <summary>
/// <para>
/// This inspired by the of answers from this question
/// https://stackoverflow.com/questions/19147/what-is-the-correct-way-to-create-a-single-instance-wpf-application
/// </para>
/// <para>
/// And this blog post
/// https://weblog.west-wind.com/posts/2016/May/13/Creating-Single-Instance-WPF-Applications-that-open-multiple-Files
/// </para>
/// </summary>
namespace MyLauncher;

public partial class App : Application
{
    public Mutex Mutex;

    public App()
    {
        SingleInstanceCheck();
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
}
