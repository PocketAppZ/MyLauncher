// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal static class WindowHelpers
{/// <summary>
 ///  Finds the parent of the child passed as a parameter
 /// </summary>
 /// <typeparam name="T"></typeparam>
 /// <param name="child"></param>
 /// <returns>parent object</returns>
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        //get parent item
        DependencyObject parentObject = VisualTreeHelper.GetParent(child);

        //we've reached the end of the tree
        if (parentObject == null) return null;

        //check if the parent matches the type we're looking for
        if (parentObject is T parent)
            return parent;
        else
            return FindParent<T>(parentObject);
    }

    /// <summary>
    /// Moves a window to the center of the PRIMARY screen
    /// </summary>
    /// <param name="window"></param>
    public static void MoveToCenter(this Window window)
    {
        double w = window.Width;
        double h = window.Height;

        window.Left = ((SystemParameters.WorkArea.Width - w) / 2) + SystemParameters.WorkArea.Left;
        window.Top = ((SystemParameters.WorkArea.Height - h) / 2) + SystemParameters.WorkArea.Top;
    }
}