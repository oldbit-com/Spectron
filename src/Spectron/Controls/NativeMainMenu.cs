using System;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public class NativeMainMenu(MainWindowViewModel viewModel)
{
    public NativeMenu Create()
    {
        var menu = new NativeMenu();

        var fileItem = new NativeMenuItem("File");

        var fileSubMenu = new NativeMenu
        {
            new NativeMenuItem("Load...")
            {
                Command = viewModel.LoadFileCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.O, KeyModifiers.Meta)
                    : new KeyGesture(Key.O, KeyModifiers.Control)
            },
            CreateRecentMenuItem(),
            new NativeMenuItem("Save Snapshot...")
            {
                Command = viewModel.SaveFileCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.S, KeyModifiers.Meta)
                    : new KeyGesture(Key.S, KeyModifiers.Control)
            },
            new NativeMenuItem("Quick Save")
            {
                Command = viewModel.QuickSaveCommand,
                Gesture = new KeyGesture(Key.F5)
            },
            new NativeMenuItem("Quick Load")
            {
                Command = viewModel.QuickLoadCommand,
                Gesture = new KeyGesture(Key.F6)
            }
        };

        fileItem.Menu = fileSubMenu;
        menu.Add(fileItem);

        return menu;
    }

    private NativeMenuItem CreateRecentMenuItem()
    {
        var recent = new NativeMenuItem("Recent...");

        var itemsMenu = new NativeMenu();
        itemsMenu.Opening += (_, _) => viewModel?.RecentFilesViewModel.Opening(itemsMenu);
        recent.Menu = itemsMenu;

        return recent;
    }
}