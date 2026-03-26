using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Models;
using OldBit.Spectron.Screen;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public class NativeMainMenu
{
    private readonly MainWindowViewModel _viewModel;

    private readonly Dictionary<ComputerType, NativeMenuItem> _computerTypes = new();
    private readonly Dictionary<RomType, NativeMenuItem> _romTypes = new();
    private readonly Dictionary<JoystickType, NativeMenuItem> _joystickTypes = new();
    private readonly Dictionary<MouseType, NativeMenuItem> _mouseTypes = new();
    private readonly Dictionary<string, NativeMenuItem> _emulationSpeeds = new();
    private readonly Dictionary<BorderSize, NativeMenuItem> _borderSizes = new();

    private NativeMenuItem? _ulaPlusMenuItem;
    private NativeMenuItem? _pauseMenuItem;
    private NativeMenuItem? _muteMenuItem;
    private NativeMenuItem? _recordAudioMenuItem;
    private NativeMenuItem? _recordVideoMenuItem;
    private NativeMenuItem? _stopRecordingMenuItem;
    private NativeMenuItem? _fullScreenMenuItem;

    public NativeMainMenu(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;

        AddComputerTypes();
        AddRomTypes();
        AddJoystickTypes();
        AddMouseTypes();
        AddSpeedOptions();
        AddBorderSizes();

        _viewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainWindowViewModel.ComputerType):
                    foreach (var computerType in _computerTypes.Keys)
                    {
                        _computerTypes[computerType].IsChecked = _viewModel.ComputerType == computerType;
                    }

                    break;

                case nameof(MainWindowViewModel.RomType):
                    foreach (var romType in _romTypes.Keys)
                    {
                        _romTypes[romType].IsChecked = _viewModel.RomType == romType;
                    }

                    break;

                case nameof(MainWindowViewModel.JoystickType):
                    foreach (var joystickType in _joystickTypes.Keys)
                    {
                        _joystickTypes[joystickType].IsChecked = _viewModel.JoystickType == joystickType;
                    }

                    break;

                case nameof(MainWindowViewModel.IsUlaPlusEnabled):
                    _ulaPlusMenuItem?.IsChecked = _viewModel.IsUlaPlusEnabled;

                    break;

                case nameof(MainWindowViewModel.EmulationSpeed):
                    foreach (var speed in _emulationSpeeds.Keys)
                    {
                        _emulationSpeeds[speed].IsChecked = _viewModel.EmulationSpeed == speed;
                    }

                    break;

                case nameof(MainWindowViewModel.IsPaused):
                    _pauseMenuItem?.IsChecked = _viewModel.IsPaused;

                    break;

                case nameof(MainWindowViewModel.IsAudioMuted):
                    _muteMenuItem?.IsChecked = _viewModel.IsAudioMuted;

                    break;

                case nameof(MainWindowViewModel.RecordingStatus):
                    _recordAudioMenuItem?.IsEnabled = _viewModel.RecordingStatus == RecordingStatus.None;
                    _recordVideoMenuItem?.IsEnabled = _viewModel.RecordingStatus == RecordingStatus.None;
                    _stopRecordingMenuItem?.IsEnabled = _viewModel.RecordingStatus != RecordingStatus.None;

                    break;

                case nameof(MainWindowViewModel.BorderSize):
                    foreach (var border in _borderSizes.Keys)
                    {
                        _borderSizes[border].IsChecked = _viewModel.BorderSize == border;
                    }

                    break;

                case nameof(MainWindowViewModel.IsFullScreen):
                    _fullScreenMenuItem?.Header = _viewModel.IsFullScreen ? "Exit Full Screen" : "Full Screen";

                    break;
            }
        };
    }

    public NativeMenu Create()
    {
        var menu = new NativeMenu
        {
            CreateFileMenuItem(),
            CreateEmulatorMenuItem(),
            CreateControlMenuItem(),
            CreateToolsMenuItem(),
            CreateViewMenuItem(),
            CreateFavoritesMenuItem(),
        };

        return menu;
    }

    private NativeMenuItem CreateFileMenuItem()
    {
        var fileItem = new NativeMenuItem("_File");

        var fileSubMenu = new NativeMenu
        {
            new NativeMenuItem("Load...")
            {
                Command = _viewModel.LoadFileCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.O, KeyModifiers.Meta)
                    : new KeyGesture(Key.O, KeyModifiers.Control)
            },
            CreateRecentMenuItem(),
            new NativeMenuItem("Save Snapshot...")
            {
                Command = _viewModel.SaveFileCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.S, KeyModifiers.Meta)
                    : new KeyGesture(Key.S, KeyModifiers.Control)
            },
            new NativeMenuItem("Quick Save")
            {
                Command = _viewModel.QuickSaveCommand,
                Gesture = new KeyGesture(Key.F5)
            },
            new NativeMenuItem("Quick Load")
            {
                Command = _viewModel.QuickLoadCommand,
                Gesture = new KeyGesture(Key.F6)
            }
        };

        fileItem.Menu = fileSubMenu;

        return fileItem;
    }

    private NativeMenuItem CreateEmulatorMenuItem()
    {
        var emulatorItem = new NativeMenuItem("_Emulator");

        _ulaPlusMenuItem = new NativeMenuItem("Enable ULA+")
        {
            ToggleType = NativeMenuItemToggleType.CheckBox,
            Command = _viewModel.ToggleUlaPlusCommand,
            IsChecked = _viewModel.IsUlaPlusEnabled,
        };

        var emulatorSubMenu = new NativeMenu
        {
            new NativeMenuItem("Machine")
            {
                Menu =
                [
                    _computerTypes[ComputerType.Spectrum16K],
                    _computerTypes[ComputerType.Spectrum48K],
                    _computerTypes[ComputerType.Spectrum128K]
                ]
            },

            new NativeMenuItem("ROM")
            {
                Menu =
                [
                    _romTypes[RomType.Original],
                    _romTypes[RomType.Pentagon128],
                    _romTypes[RomType.GoshWonderful],
                    _romTypes[RomType.BusySoft],
                    _romTypes[RomType.Harston],
                    _romTypes[RomType.HtrSuperBasic],
                    _romTypes[RomType.PrettyBasic],
                    _romTypes[RomType.BbcBasic],

                    new NativeMenuItem("Diagnostics")
                    {
                        Menu =
                        [
                            _romTypes[RomType.Retroleum],
                            _romTypes[RomType.BrendanAlford],
                        ]
                    },

                    _romTypes[RomType.Custom],
                ]
            },

            new NativeMenuItem("Joystick")
            {
                Menu =
                [
                    _joystickTypes[JoystickType.None],
                    _joystickTypes[JoystickType.Kempston],
                    _joystickTypes[JoystickType.Sinclair1],
                    _joystickTypes[JoystickType.Sinclair2],
                    _joystickTypes[JoystickType.Cursor],
                    _joystickTypes[JoystickType.Fuller],
                ]
            },

            new NativeMenuItem("Mouse")
            {
                Menu =
                [
                    _mouseTypes[MouseType.None],
                    _mouseTypes[MouseType.Kempston],
                ]
            },

            new NativeMenuItemSeparator(),

            _ulaPlusMenuItem,
        };

        emulatorItem.Menu = emulatorSubMenu;

        return emulatorItem;
    }

    private NativeMenuItem CreateControlMenuItem()
    {
        var controlItem = new NativeMenuItem("Control");

        _pauseMenuItem = new NativeMenuItem("Pause")
        {
            ToggleType = NativeMenuItemToggleType.CheckBox,
            Command = _viewModel.TogglePauseCommand,
            Gesture = new KeyGesture(Key.F2),
            IsChecked = _viewModel.IsPaused,
        };

        _muteMenuItem = new NativeMenuItem("Mute")
        {
            ToggleType = NativeMenuItemToggleType.CheckBox,
            Command = _viewModel.ToggleMuteCommand,
            IsChecked = _viewModel.IsAudioMuted,
        };

        var controlSubMenu = new NativeMenu
        {
            new NativeMenuItem("Speed")
            {
                Menu =
                [
                    _emulationSpeeds["25"],
                    _emulationSpeeds["75"],
                    _emulationSpeeds["100"],
                    _emulationSpeeds["125"],
                    _emulationSpeeds["150"],
                    _emulationSpeeds["200"],
                    _emulationSpeeds["250"],
                    _emulationSpeeds["300"],
                    _emulationSpeeds["400"],
                    _emulationSpeeds["500"],
                    _emulationSpeeds["Max"],
                ]
            },

            new NativeMenuItemSeparator(),

            _pauseMenuItem,

            new NativeMenuItem("Time Machine")
            {
                Command = _viewModel.ShowTimeMachineViewCommand,
                Gesture = new KeyGesture(Key.F3)
            },

            new NativeMenuItemSeparator(),

            _muteMenuItem,

            new NativeMenuItemSeparator(),

            new NativeMenuItem("NMI")
            {
                Command = _viewModel.TriggerNmiCommand,
            },

            new NativeMenuItem("Reset")
            {
                Command = _viewModel.ResetCommand,
                Gesture = new KeyGesture(Key.F3, KeyModifiers.Control)
            },

            new NativeMenuItem("Hard Reset")
            {
                Command = _viewModel.HardResetCommand,
            },
        };

        controlItem.Menu = controlSubMenu;

        return controlItem;
    }

    private NativeMenuItem CreateToolsMenuItem()
    {
        var toolsItem = new NativeMenuItem("Tools");

        _recordAudioMenuItem = new NativeMenuItem("Record Audio...")
        {
            Command = _viewModel.StartAudioRecordingCommand,
            IsEnabled = _viewModel.RecordingStatus == RecordingStatus.None
        };

        _recordVideoMenuItem = new NativeMenuItem("Record Video...")
        {
            Command = _viewModel.StartVideoRecordingCommand,
            IsEnabled = _viewModel.RecordingStatus == RecordingStatus.None
        };

        _stopRecordingMenuItem = new NativeMenuItem("Stop Recording")
        {
            Command = _viewModel.StopRecordingCommand,
            IsEnabled = _viewModel.RecordingStatus != RecordingStatus.None
        };

        var toolsSubMenu = new NativeMenu
        {
            _recordAudioMenuItem,
            _recordVideoMenuItem,
            _stopRecordingMenuItem,

            new NativeMenuItemSeparator(),

            new NativeMenuItem("Take Screenshot")
            {
                Command = _viewModel.TakeScreenshotCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.C, KeyModifiers.Meta)
                    : new KeyGesture(Key.C, KeyModifiers.Control)
            },

            new NativeMenuItem("View Screenshots")
            {
                Command = _viewModel.ShowScreenshotViewerCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.C, KeyModifiers.Meta | KeyModifiers.Shift)
                    : new KeyGesture(Key.C, KeyModifiers.Control | KeyModifiers.Shift)
            }
        };

        toolsItem.Menu = toolsSubMenu;

        return toolsItem;
    }

    private NativeMenuItem CreateViewMenuItem()
    {
        var viewItem = new NativeMenuItem("View");

        _fullScreenMenuItem = new NativeMenuItem("Full Screen")
        {
            Command = _viewModel.ToggleFullScreenCommand,
            Gesture = new KeyGesture(Key.Enter, KeyModifiers.Alt | KeyModifiers.Shift)
        };

        var viewSubMenu = new NativeMenu
        {
            new NativeMenuItem("Border")
            {
                Menu =
                [
                    _borderSizes[BorderSize.None],
                    _borderSizes[BorderSize.Small],
                    _borderSizes[BorderSize.Medium],
                    _borderSizes[BorderSize.Large],
                    _borderSizes[BorderSize.Full],
                ]
            },

            new NativeMenuItem("Trainers")
            {
                Command = _viewModel.ShowTrainersCommand,
            },

            new NativeMenuItem("Print Output")
            {
                Command = _viewModel.ShowPrintOutputCommand,
            },

            new NativeMenuItemSeparator(),

            _fullScreenMenuItem,
        };

        viewItem.Menu = viewSubMenu;

        return viewItem;
    }

    private NativeMenuItem CreateFavoritesMenuItem()
    {
        var viewItem = new NativeMenuItem("Favourites");

        var itemsMenu = new NativeMenu
        {
            new NativeMenuItem("Edit...")
            {
                Command = _viewModel.ShowFavoritesViewCommand,
            },
        };

        itemsMenu.Opening += (_, _) => _viewModel.FavoritesViewModel.Opening(itemsMenu);

        viewItem.Menu = itemsMenu;

        return viewItem;
    }

    private NativeMenuItem CreateRecentMenuItem()
    {
        var recentItem = new NativeMenuItem("Recent...");

        var itemsMenu = new NativeMenu();
        itemsMenu.Opening += (_, _) => _viewModel.RecentFilesViewModel.Opening(itemsMenu);

        recentItem.Menu = itemsMenu;

        return recentItem;
    }

    private void AddComputerTypes()
    {
        var computers = new[]
        {
            new { ComputerType = ComputerType.Spectrum16K, DisplayName = "ZX Spectrum 16" },
            new { ComputerType = ComputerType.Spectrum48K, DisplayName = "ZX Spectrum 48" },
            new { ComputerType = ComputerType.Spectrum128K, DisplayName = "ZX Spectrum 128" },
        };

        foreach (var computer in computers)
        {
            _computerTypes[computer.ComputerType] = new NativeMenuItem(computer.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.ChangeComputerTypeCommand,
                CommandParameter = computer.ComputerType,
                IsChecked = _viewModel.ComputerType == computer.ComputerType,
                IsEnabled = true,
            };
        }
    }

    private void AddRomTypes()
    {
        var roms = new[]
        {
            new { RomType = RomType.Original, DisplayName = "Original" },
            new { RomType = RomType.Pentagon128, DisplayName = "Pentagon (128 mode only)" },
            new { RomType = RomType.GoshWonderful, DisplayName = "Gosh Wonderful" },
            new { RomType = RomType.BusySoft, DisplayName = "Busy Soft v1.40" },
            new { RomType = RomType.Harston, DisplayName = "J.G. Harston v0.77" },
            new { RomType = RomType.HtrSuperBasic, DisplayName = "H.T.R. SuperBasic" },
            new { RomType = RomType.PrettyBasic, DisplayName = "Pretty Basic" },
            new { RomType = RomType.BbcBasic, DisplayName = "BBC Basic (128 mode only)" },
            new { RomType = RomType.Retroleum, DisplayName = "Retroleum v1.71" },
            new { RomType = RomType.BrendanAlford, DisplayName = "B. Alford v1.37" },
            new { RomType = RomType.Custom, DisplayName = "Custom" },
        };

        foreach (var rom in roms)
        {
            _romTypes[rom.RomType] = new NativeMenuItem(rom.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.ChangeRomCommand,
                CommandParameter = rom.RomType,
                IsChecked = _viewModel.RomType == rom.RomType,
                IsEnabled = true
            };
        }
    }

    private void AddJoystickTypes()
    {
        var joysticks = new[]
        {
            new { JoystickType = JoystickType.None, DisplayName = "None" },
            new { JoystickType = JoystickType.Kempston, DisplayName = "Kempston" },
            new { JoystickType = JoystickType.Sinclair1, DisplayName = "Sinclair 1" },
            new { JoystickType = JoystickType.Sinclair2, DisplayName = "Sinclair 2" },
            new { JoystickType = JoystickType.Cursor, DisplayName = "Cursor" },
            new { JoystickType = JoystickType.Fuller, DisplayName = "Fuller" },
        };

        foreach (var joystick in joysticks)
        {
            _joystickTypes[joystick.JoystickType] = new NativeMenuItem(joystick.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.ChangeJoystickTypeCommand,
                CommandParameter = joystick.JoystickType,
                IsChecked = _viewModel.JoystickType == joystick.JoystickType,
                IsEnabled = true
            };
        }
    }

    private void AddMouseTypes()
    {
        var mice = new[]
        {
            new { MouseType = MouseType.None, DisplayName = "None" },
            new { MouseType = MouseType.Kempston, DisplayName = "Kempston" },
        };

        foreach (var mouse in mice)
        {
            _mouseTypes[mouse.MouseType] = new NativeMenuItem(mouse.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.ChangeJoystickTypeCommand,
                CommandParameter = mouse.MouseType,
                IsChecked = _viewModel.MouseType == mouse.MouseType,
                IsEnabled = true
            };
        }
    }

    private void AddSpeedOptions()
    {
        var speeds = new[]
        {
            new { Value = "25", DisplayName = "25%" },
            new { Value = "50", DisplayName = "50%" },
            new { Value = "75", DisplayName = "75%" },
            new { Value = "100", DisplayName = "100%" },
            new { Value = "125", DisplayName = "125%" },
            new { Value = "150", DisplayName = "150%" },
            new { Value = "200", DisplayName = "200%" },
            new { Value = "250", DisplayName = "250%" },
            new { Value = "300", DisplayName = "300%" },
            new { Value = "400", DisplayName = "400%" },
            new { Value = "500", DisplayName = "500%" },
            new { Value = "Max", DisplayName = "Max" },
        };

        foreach (var speed in speeds)
        {
            _emulationSpeeds[speed.Value] = new NativeMenuItem(speed.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.SetEmulationSpeedCommand,
                CommandParameter = speed.Value,
                IsChecked = _viewModel.EmulationSpeed == speed.Value,
                IsEnabled = true
            };
        }
    }

    private void AddBorderSizes()
    {
        var borders = new[]
        {
            new { Size = BorderSize.None, DisplayName = "None" },
            new { Size = BorderSize.Small, DisplayName = "Small" },
            new { Size = BorderSize.Medium, DisplayName = "Medium" },
            new { Size = BorderSize.Large, DisplayName = "Large" },
            new { Size = BorderSize.Full, DisplayName = "Full" },
        };

        foreach (var border in borders)
        {
            _borderSizes[border.Size] = new NativeMenuItem(border.DisplayName)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                Command = _viewModel.ChangeBorderSizeCommand,
                CommandParameter = border.Size,
                IsChecked = _viewModel.BorderSize == border.Size,
                IsEnabled = true
            };
        }
    }
}