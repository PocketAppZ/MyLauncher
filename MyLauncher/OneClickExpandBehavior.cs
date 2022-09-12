// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
///   Attached property used to expand/collapse a TreeView node on a single mouse click
///   Adapted from https://stackoverflow.com/a/33894758/15237757
/// </summary>
public class OneClickExpandBehavior : DependencyObject
{
    public static bool GetEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnabledProperty);
    }

    public static void SetEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(EnabledProperty, value);
    }

    public static readonly DependencyProperty EnabledProperty =
        DependencyProperty.RegisterAttached(name: "Enabled",
                           propertyType: typeof(bool),
                           ownerType: typeof(OneClickExpandBehavior),
                           defaultMetadata: new UIPropertyMetadata(defaultValue: false,
                                                                   propertyChangedCallback: EnabledPropertyChangedCallback
                ));

    private static void EnabledPropertyChangedCallback(DependencyObject dependencyObject,
                                                       DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        if (dependencyObject is TreeView treeView)
        {
            treeView.MouseUp += TreeView_MouseUp;
        }
    }

    private static void TreeView_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) is TreeViewItem treeViewItem)
        {
            treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
        }
    }

    private static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
    {
        while (source != null && source.GetType() != typeof(T))
        {
            source = VisualTreeHelper.GetParent(source);
        }
        return source;
    }
}
