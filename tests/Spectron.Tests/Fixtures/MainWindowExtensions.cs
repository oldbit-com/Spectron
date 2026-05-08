using Avalonia.Controls;
using Avalonia.LogicalTree;
using OldBit.Spectron.Views;

namespace OldBit.Spectron.Tests.Fixtures;

public static class MainWindowExtensions
{
    public static MenuItem? FindMenuItem(this MainWindow window, string header) => window
        .GetLogicalDescendants().OfType<MenuItem>()
        .FirstOrDefault(x => Equals(x.Header, header));
}