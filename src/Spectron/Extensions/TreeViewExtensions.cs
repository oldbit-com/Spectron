using Avalonia.Controls;

namespace OldBit.Spectron.Extensions;

public static class TreeViewExtensions
{
    public static TreeViewItem? FindContainer(this ItemsControl parent, object targetItem)
    {
        if (parent.ContainerFromItem(targetItem) is TreeViewItem rootContainer)
        {
            return rootContainer;
        }

        foreach (var item in parent.Items)
        {
            if (item is null || parent.ContainerFromItem(item) is not TreeViewItem childContainer)
            {
                continue;
            }

            var result = childContainer.FindContainer(targetItem);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}