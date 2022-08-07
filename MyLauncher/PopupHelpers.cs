// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

internal static class PopupHelpers
{
    #region Find pop-up by ID
    /// <summary>
    /// Finds a pop-up by its ItemID
    /// </summary>
    /// <param name="collection">Collection to search</param>
    /// <param name="itemID">ItemID of the pop-up</param>
    /// <returns>MyListItem object for the pop-up</returns>
    internal static MyListItem FindPopup(ObservableCollection<MyListItem> collection, string itemID)
    {
        if (collection is not null)
        {
            MyListItem result = collection.FirstOrDefault(f => f.ItemID == itemID);

            if (result is null)
            {
                foreach (var item in collection)
                {
                    result = FindPopup(item.MyListItems, itemID);
                    if (result is not null)
                    {
                        break;
                    }
                }
            }
            return result;
        }
        return null;
    }
    #endregion Find pop-up by ID

    #region Get list of pop-ups
    /// <summary>
    /// Get a list of pop-ups sorted by title
    /// </summary>
    /// <returns>List of MyListItem pop-up objects</returns>
    internal static List<MyListItem> SortedPopups()
    {
        List<MyListItem> PopupsTemp = new();
        return GetPopups(MyListItem.Children, PopupsTemp).OrderBy(x => x.Title).ToList();
    }

    private static List<MyListItem> GetPopups(ObservableCollection<MyListItem> collection, List<MyListItem> results)
    {
        if (collection is not null)
        {
            foreach (MyListItem item in collection)
            {
                if (item.EntryType == ListEntryType.Popup)
                {
                    results.Add(item);
                }
                if (item.MyListItems is not null)
                {
                    GetPopups(item.MyListItems, results);
                }
            }
            return results;
        }
        return null;
    }
    #endregion Get list of pop-ups

}