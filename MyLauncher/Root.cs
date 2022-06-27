// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

/// <summary>
/// Root class for deserializing the JSON
/// </summary>
public class Root
{
    public ObservableCollection<Child> Children { get; set; }
}
