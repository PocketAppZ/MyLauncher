// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;

namespace MyLauncher;

/// <summary>
/// Class to open text files in the default application for the file type.
/// If there isn't a default, open the file in notepad.exe
/// </summary>
internal static class TextFileViewer
{
    #region NLog Instance
    //private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    #region Text file viewer
    /// <summary>
    /// Open the file in the default application
    /// </summary>
    /// <param name="txtfile">File to open</param>
    public static void ViewTextFile(string txtfile)
    {
        if (File.Exists(txtfile))
        {
            try
            {
                using Process p = new();
                p.StartInfo.FileName = txtfile;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.ErrorDialog = false;
                _ = p.Start();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1155)
                {
                    using Process p = new();
                    p.StartInfo.FileName = "notepad.exe";
                    p.StartInfo.Arguments = txtfile;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.ErrorDialog = false;
                    _ = p.Start();
                }
                else
{
                    SystemSounds.Exclamation.Play();
                    _ = MessageBox.Show($"Unable to open {txtfile}.\n{ex.Message}",
                        "ERROR",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                SystemSounds.Exclamation.Play();
                _ = MessageBox.Show($"Unable to open {txtfile}.\n{ex.Message}",
                    "ERROR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        else
        {
            SystemSounds.Exclamation.Play();
            _ = MessageBox.Show($"File not Found {txtfile}.",
                "ERROR",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    #endregion Text file viewer
}
