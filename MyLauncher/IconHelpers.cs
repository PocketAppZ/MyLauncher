// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Class used to get the images for the listbox items.
/// </summary>
internal static class IconHelpers
{
    #region NLog Instance
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    #region Get the images (icons)
    /// <summary>
    /// Find a suitable image for the item
    /// </summary>
    /// <param name="ch"></param>
    public static void GetIcons(ObservableCollection<Child> ch)
    {
        if (ch is not null)
        {
            foreach (Child item in ch)
            {
                if (!string.IsNullOrEmpty(item.FilePathOrURI) || item.EntryType == ListEntryType.Popup)
                {
                    if (!string.IsNullOrEmpty(item.IconSource))
                    {
                        string image = Path.Combine(AppInfo.AppDirectory, "Icons", item.IconSource);
                        if (File.Exists(image))
                        {
                            log.Debug($"Using image file {item.IconSource} for \"{item.Title}\".");
                            BitmapImage bmi = new();
                            bmi.BeginInit();
                            bmi.UriSource = new Uri(image);
                            bmi.EndInit();
                            item.FileIcon = bmi;
                            //log.Debug($"Image size is {bmi.PixelHeight} x {bmi.PixelWidth}");
                            continue;
                        }
                        log.Debug($"Could not find file {image} to use for \"{item.Title}\".");
                    }

                    if (item.EntryType == ListEntryType.Popup)
                    {
                        string image = Path.Combine(AppInfo.AppDirectory, "Icons", "UpArrow.png");
                        if (File.Exists(image))
                        {
                            log.Debug($"Using image file UpArrow.png for \"{item.Title}\".");
                            BitmapImage bmi = new();
                            bmi.BeginInit();
                            bmi.UriSource = new Uri(image);
                            bmi.EndInit();
                            item.FileIcon = bmi;
                            continue;
                        }
                        log.Debug($"Could not find file {image} to use for \"{item.Title}\".");
                    }

                    string filePath = item.FilePathOrURI.TrimEnd('\\');
                    if (File.Exists(filePath))
                    {
                        if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                        {
                            string shortcut = ((IWshShortcut)new WshShell().CreateShortcut(filePath)).TargetPath;
                            Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(shortcut);
                            item.FileIcon = IconToImageSource(temp);
                            log.Debug($"Using extracted associated icon from shortcut to {item.FilePathOrURI}.");
                        }
                        else
                        {
                            Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                            item.FileIcon = IconToImageSource(temp);
                            log.Debug($"Using extracted associated icon for {item.FilePathOrURI}.");
                        }
                    }
                    // expand environmental variables for folders
                    else if (Directory.Exists(Environment.ExpandEnvironmentVariables(filePath)))
                    {
                        Icon temp = Properties.Resources.folder;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"Using folder icon for {item.FilePathOrURI}.");
                    }
                    // if complete path wasn't supplied check the path
                    else if (filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        StringBuilder sb = new(filePath, 2048);
                        bool found = NativeMethods.PathFindOnPath(sb, new string[] { null });
                        if (found)
                        {
                            Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(sb.ToString());
                            item.FileIcon = IconToImageSource(temp);
                            log.Debug($"Using extracted associated icon for {item.FilePathOrURI}.");
                        }
                        else
                        {
                            Icon temp = Properties.Resources.question;
                            item.FileIcon = IconToImageSource(temp);
                            log.Warn($"Icon for {item.FilePathOrURI} could not be located.");
                        }
                    }
                    // maybe it's a url
                    else if (IsValidUrl(filePath))
                    {
                        Icon temp = Properties.Resources.globe;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"Using globe icon for {item.FilePathOrURI}. It appears to be a valid URL.");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question;
                        item.FileIcon = IconToImageSource(temp);
                        log.Warn($"Icon for {item.FilePathOrURI} could not be located.");
                    }
                }
                // this shouldn't happen
                else
                {
                    Icon temp = Properties.Resources.question;
                    item.FileIcon = IconToImageSource(temp);
                    log.Warn("Path is empty or null. Icon cannot be assigned.");
                }
            }
        }
    }
    #endregion Get the images (icons)

    #region Convert icon to image source
    /// <summary>
    /// Converts an Icon to ImageSource
    /// </summary>
    /// <param name="icon">The icon to be converted</param>
    /// <returns>ImageSource image</returns>
    private static ImageSource IconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            new Int32Rect(0, 0, icon.Width, icon.Height),
            BitmapSizeOptions.FromEmptyOptions());
    }
    #endregion Convert icon to image source

    #region Check URL
    /// <summary>
    /// Rudimentary method to validate a URL
    /// </summary>
    /// <param name="uriName"></param>
    /// <returns></returns>
    private static bool IsValidUrl(string uriName)
    {
        const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        Regex Rgx = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return Rgx.IsMatch(uriName);
    }
    #endregion Check URL
}