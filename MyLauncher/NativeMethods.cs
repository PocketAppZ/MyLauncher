// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal static class NativeMethods
{
    #region Methods used by single instance check in App.xaml.cs
    [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern Boolean IsIconic([In] IntPtr windowHandle);

    [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern Boolean SetForegroundWindow([In] IntPtr windowHandle);

    [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern Boolean ShowWindow([In] IntPtr windowHandle, [In] ShowWindowCommand command);

    public enum ShowWindowCommand
    {
        Hide = 0x0,
        ShowNormal = 0x1,
        ShowMinimized = 0x2,
        ShowMaximized = 0x3,
        ShowNormalNotActive = 0x4,
        Minimize = 0x6,
        ShowMinimizedNotActive = 0x7,
        ShowCurrentNotActive = 0x8,
        Restore = 0x9,
        ShowDefault = 0xA,
        ForceMinimize = 0xB
    }
    #endregion Methods used by single instance check in App.xaml.cs

    #region Find file on Path
    /// <summary>
    /// File a file on the system path
    /// </summary>
    /// <param name="pszFile">File to search for</param>
    /// <param name="ppszOtherDirs"></param>
    /// <returns></returns>
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
    public static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);
    #endregion Find file on Path

    #region Goodbye window buttons
    /// <summary>
    /// Method to remove the minimize and maximize/restore buttons
    /// </summary>
    /// <param name="windowHandle"></param>
    internal static void DisableMinMaxButtons(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }
        _ = SetWindowLong(windowHandle,
                          GWL_STYLE,
                          GetWindowLong(windowHandle, GWL_STYLE) & ~WS_BOTHBOXES);
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;                        // maximize button
    private const int WS_MINIMIZEBOX = 0x20000;                        // minimize button
    private const int WS_BOTHBOXES = WS_MINIMIZEBOX + WS_MAXIMIZEBOX;  // Both
    #endregion Goodbye window buttons
}
