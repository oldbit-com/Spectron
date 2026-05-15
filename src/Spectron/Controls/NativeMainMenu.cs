using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Screen;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public sealed class NativeMainMenu
{
    private readonly MainViewModel _viewModel;
    private readonly NativeMenu _nativeMenu = [];
    private NativeMenuItem[] _rootItems = [];

    private readonly Dictionary<ComputerType, NativeMenuItem> _computerTypes = new();
    private readonly Dictionary<RomType, NativeMenuItem> _romTypes = new();
    private readonly Dictionary<JoystickType, NativeMenuItem> _joystickTypes = new();
    private readonly Dictionary<MouseType, NativeMenuItem> _mouseTypes = new();
    private readonly Dictionary<int, NativeMenuItem> _emulationSpeeds = new();
    private readonly Dictionary<int, NativeMenuItem> _clockMultipliers = new();
    private readonly Dictionary<BorderSize, NativeMenuItem> _borderSizes = new();
    private readonly Dictionary<ScreenEffect, NativeMenuItem> _screenEffects = new();
    private readonly Dictionary<TapeSpeed, NativeMenuItem> _tapeLoadingSpeeds = new();
    private readonly Dictionary<MicrodriveId, NativeMenuItem> _microdrives = new();
    private readonly Dictionary<DriveId, NativeMenuItem> _diskDrives = new();

    private NativeMenuItem? _ulaPlusMenuItem;
    private NativeMenuItem? _pauseMenuItem;
    private NativeMenuItem? _muteMenuItem;
    private NativeMenuItem? _fullScreenMenuItem;
    private NativeMenuItem? _microdriveMenuItem;
    private NativeMenuItem? _diskDriveMenuItem;
    private NativeMenuItem? _debuggerBreakpointMenuItem;

    public NativeMainMenu(MainViewModel viewModel)
    {
        _viewModel = viewModel;

        CreateComputerTypeMenu();
        CreateRomTypeMenu();
        CreateJoystickTypeMenu();
        CreateMouseTypeMenu();
        CreateSpeedOptionMenu();
        CreateClockMultiplierOptionMenu();
        CreateBorderSizeMenu();
        CreateScreenEffectMenu();
        CreateTapeLoadingSpeedMenu();

        _viewModel.PropertyChanged += (_, e) => ViewModelPropertyChanged(e.PropertyName);
    }

    public NativeMenu Create()
    {
        if (_rootItems.Length == 0)
        {
            _rootItems =
            [
                CreateFileMenu(),
                CreateEmulatorMenu(),
                CreateControlMenu(),
                CreateToolsMenu(),
                CreateViewMenu(),
                CreateFavoritesMenu(),
                CreateTapeMenu(),
                CreateMicrodriveMenu(),
                CreateDiskDriveMenu(),
                CreateDebugMenu(),
                CreateHelpMenu(),
            ];
        }

        _nativeMenu.Items.AddRange(_rootItems);

        return _nativeMenu;
    }

    public NativeMenu Empty()
    {
        _nativeMenu.Items.Clear();

        return _nativeMenu;
    }

    private void ViewModelPropertyChanged(string? propertyName)
    {
        switch (propertyName)
            {
                case nameof(MainViewModel.ComputerType):
                    foreach (var computerType in _computerTypes.Keys)
                    {
                        _computerTypes[computerType].IsChecked = _viewModel.ComputerType == computerType;
                    }

                    break;

                case nameof(MainViewModel.RomType):
                    foreach (var romType in _romTypes.Keys)
                    {
                        _romTypes[romType].IsChecked = _viewModel.RomType == romType;
                    }

                    break;

                case nameof(MainViewModel.JoystickType):
                    foreach (var joystickType in _joystickTypes.Keys)
                    {
                        _joystickTypes[joystickType].IsChecked = _viewModel.JoystickType == joystickType;
                    }

                    break;

                case nameof(MainViewModel.MouseType):
                    foreach (var mouseType in _mouseTypes.Keys)
                    {
                        _mouseTypes[mouseType].IsChecked = _viewModel.MouseType == mouseType;
                    }

                    break;

                case nameof(MainViewModel.IsUlaPlusEnabled):
                    _ulaPlusMenuItem?.IsChecked = _viewModel.IsUlaPlusEnabled;

                    break;

                case nameof(MainViewModel.EmulationSpeed):
                    foreach (var speed in _emulationSpeeds.Keys)
                    {
                        _emulationSpeeds[speed].IsChecked = _viewModel.EmulationSpeed == speed;
                    }

                    break;

                case nameof(MainViewModel.ClockMultiplier):
                    foreach (var multiplier in _clockMultipliers.Keys)
                    {
                        _clockMultipliers[multiplier].IsChecked = _viewModel.ClockMultiplier == multiplier;
                    }

                    break;

                case nameof(MainViewModel.IsPaused):
                    _pauseMenuItem?.IsChecked = _viewModel.IsPaused;

                    break;

                case nameof(MainViewModel.IsAudioMuted):
                    _muteMenuItem?.IsChecked = _viewModel.IsAudioMuted;

                    break;

                case nameof(MainViewModel.BorderSize):
                    foreach (var border in _borderSizes.Keys)
                    {
                        _borderSizes[border].IsChecked = _viewModel.BorderSize == border;
                    }

                    break;

                case nameof(MainViewModel.ScreenEffect):
                    foreach (var screenEffect in _screenEffects.Keys)
                    {
                        _screenEffects[screenEffect].IsChecked = _viewModel.ScreenEffect.HasFlag(screenEffect);
                    }

                    break;

                case nameof(MainViewModel.IsFullScreen):
                    _fullScreenMenuItem?.Header = _viewModel.IsFullScreen ? "Exit Full Screen" : "Full Screen";

                    break;

                case nameof(MainViewModel.TapeLoadSpeed):
                    foreach (var speed in _tapeLoadingSpeeds.Keys)
                    {
                        _tapeLoadingSpeeds[speed].IsChecked = _viewModel.TapeLoadSpeed == speed;
                    }

                    break;

                case nameof(MainViewModel.IsInterface1Enabled):
                    _microdriveMenuItem?.IsVisible = _viewModel.IsInterface1Enabled;

                    break;

                case nameof(MainViewModel.NumberOfMicrodrives):
                    foreach (var driveId in _microdrives.Keys)
                    {
                        _microdrives[driveId].IsVisible = _viewModel.NumberOfMicrodrives >= (int)driveId;
                    }

                    break;

                case nameof(MainViewModel.IsBeta128Enabled):
                    _diskDriveMenuItem?.IsVisible = _viewModel.IsBeta128Enabled;

                    break;

                case nameof(MainViewModel.NumberOfBeta128Drives):
                    foreach (var driveId in _diskDrives.Keys)
                    {
                        _diskDrives[driveId].IsVisible = _viewModel.NumberOfBeta128Drives >= (int)driveId;
                    }

                    break;

                case nameof(MainViewModel.BreakpointsEnabled):
                    _debuggerBreakpointMenuItem?.IsChecked = _viewModel.BreakpointsEnabled;

                    break;
            }
    }

    private NativeMenuItem CreateFileMenu()
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

    private NativeMenuItem CreateEmulatorMenu()
    {
        var emulatorItem = new NativeMenuItem("_Emulator");

        _ulaPlusMenuItem = new NativeMenuItem("Enable ULA+")
        {
            ToggleType = MenuItemToggleType.CheckBox,
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
                    _computerTypes[ComputerType.Spectrum128K],
                    _computerTypes[ComputerType.Timex2048],
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

    private NativeMenuItem CreateControlMenu()
    {
        var controlItem = new NativeMenuItem("Control");

        _pauseMenuItem = new NativeMenuItem("Pause")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = _viewModel.TogglePauseCommand,
            Gesture = new KeyGesture(Key.F2),
            IsChecked = _viewModel.IsPaused,
        };

        _muteMenuItem = new NativeMenuItem("Mute")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = _viewModel.ToggleMuteCommand,
            IsChecked = _viewModel.IsAudioMuted,
        };

        var controlSubMenu = new NativeMenu
        {
            new NativeMenuItem("Emulator Speed")
            {
                Menu =
                [
                    _emulationSpeeds[25],
                    _emulationSpeeds[50],
                    _emulationSpeeds[75],
                    _emulationSpeeds[100],
                    _emulationSpeeds[125],
                    _emulationSpeeds[150],
                    _emulationSpeeds[200],
                    _emulationSpeeds[250],
                    _emulationSpeeds[300],
                    _emulationSpeeds[400],
                    _emulationSpeeds[500],
                    _emulationSpeeds[-1],
                ]
            },

            new NativeMenuItem("CPU Clock")
            {
                Menu =
                [
                    _clockMultipliers[1],
                    _clockMultipliers[2],
                    _clockMultipliers[4],
                    _clockMultipliers[8],
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

    private NativeMenuItem CreateToolsMenu()
    {
        var toolsItem = new NativeMenuItem("Tools");

        var toolsSubMenu = new NativeMenu
        {
            new NativeMenuItem("Record Audio...")
            {
                Command = _viewModel.StartAudioRecordingCommand
            },

            new NativeMenuItem("Record Video...")
            {
                Command = _viewModel.StartVideoRecordingCommand,
            },

            new NativeMenuItem("Stop Recording")
            {
                Command = _viewModel.StopRecordingCommand,
            },

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

    private NativeMenuItem CreateViewMenu()
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

            new NativeMenuItem("Effect")
            {
                Menu =
                [
                    _screenEffects[ScreenEffect.Blur],
                    _screenEffects[ScreenEffect.Crt],
                ]
            },

            new NativeMenuItemSeparator(),

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

    private NativeMenuItem CreateTapeMenu()
    {
        var tapeItem = new NativeMenuItem("Tape");

        var tapeSubMenu = new NativeMenu
        {
            new NativeMenuItem("New")
            {
                Command = _viewModel.TapeMenuViewModel.NewCommand,
            },

            new NativeMenuItem("Insert...")
            {
                Command = _viewModel.TapeMenuViewModel.InsertCommand,
                Gesture = OperatingSystem.IsMacOS()
                    ? new KeyGesture(Key.T, KeyModifiers.Meta)
                    : new KeyGesture(Key.T, KeyModifiers.Control)
            },

            new NativeMenuItem("Save...")
            {
                Command = _viewModel.TapeMenuViewModel.SaveCommand,
            },

            new NativeMenuItemSeparator(),

            new NativeMenuItem("Play")
            {
                Command = _viewModel.TapeMenuViewModel.PlayCommand,
                IsEnabled = _viewModel.TapeMenuViewModel.PlayCommand.CanExecute(null),
            },

            new NativeMenuItem("Stop")
            {
                Command = _viewModel.TapeMenuViewModel.StopCommand,
                IsEnabled = _viewModel.TapeMenuViewModel.StopCommand.CanExecute(null),
            },

            new NativeMenuItem("Rewind")
            {
                Command = _viewModel.TapeMenuViewModel.RewindCommand,
                IsEnabled = _viewModel.TapeMenuViewModel.RewindCommand.CanExecute(null),
            },

            new NativeMenuItem("Eject")
            {
                Command = _viewModel.TapeMenuViewModel.EjectCommand,
                IsEnabled = _viewModel.TapeMenuViewModel.EjectCommand.CanExecute(null),
            },

            new NativeMenuItem("View")
            {
                Command = _viewModel.TapeMenuViewModel.ViewCommand,
                IsEnabled = _viewModel.TapeMenuViewModel.ViewCommand.CanExecute(null),
            },

            new NativeMenuItemSeparator(),

            new NativeMenuItem("Loading Speed")
            {
                Menu =
                [
                    _tapeLoadingSpeeds[TapeSpeed.Normal],
                    _tapeLoadingSpeeds[TapeSpeed.Instant],
                    _tapeLoadingSpeeds[TapeSpeed.Accelerated],
                ]
            }
        };

        tapeItem.Menu = tapeSubMenu;

        return tapeItem;
    }

    private NativeMenuItem CreateMicrodriveMenu()
    {
        _microdriveMenuItem = new NativeMenuItem("Microdrive")
        {
            IsVisible = _viewModel.IsInterface1Enabled
        };

        var microdriveSubMenu = new NativeMenu();

        for (var i = 0; i < 8; i++)
        {
            var driveId = (MicrodriveId)(i + 1);

            var driveItem = new NativeMenuItem($"Drive {i + 1}")
            {
                Menu =
                [
                    new NativeMenuItem("New")
                    {
                        Command = _viewModel.MicrodriveMenuViewModel.NewCommand,
                        CommandParameter = driveId,
                        IsEnabled = true,
                    },

                    new NativeMenuItem("Insert...")
                    {
                        Command = _viewModel.MicrodriveMenuViewModel.InsertCommand,
                        CommandParameter = driveId,
                        IsEnabled = true,
                    },

                    new NativeMenuItem("Save...")
                    {
                        Command = _viewModel.MicrodriveMenuViewModel.SaveCommand,
                        CommandParameter = driveId,
                        IsEnabled = _viewModel.MicrodriveMenuViewModel.SaveCommand.CanExecute(null),
                    },

                    CreateMicrodriveEjectMenuItem(driveId),

                    new NativeMenuItemSeparator(),

                    CreateMicrodriveWriteProtectMenuItem(driveId),
                ],

                IsVisible = _viewModel.NumberOfMicrodrives > (int)driveId
            };

            _microdrives[driveId] = driveItem;

            microdriveSubMenu.Items.Add(driveItem);
        }

        _microdriveMenuItem.Menu = microdriveSubMenu;

        return _microdriveMenuItem;
    }

    private NativeMenuItem CreateDiskDriveMenu()
    {
        _diskDriveMenuItem = new NativeMenuItem("Disk")
        {
            IsVisible = _viewModel.IsBeta128Enabled
        };

        var diskSubMenu = new NativeMenu();

        var driveLetters = new[] { "A:", "B:", "C:", "D:"};

        for (var i = 0; i < 4; i++)
        {
            var driveId = (DriveId)(i + 1);

            var diskItem = new NativeMenuItem(driveLetters[i])
            {
                Menu =
                [
                    new NativeMenuItem("New")
                    {
                        Command = _viewModel.DiskDriveMenuViewModel.NewCommand,
                        CommandParameter = driveId,
                        IsEnabled = true,
                    },

                    new NativeMenuItem("Insert...")
                    {
                        Command = _viewModel.DiskDriveMenuViewModel.InsertCommand,
                        CommandParameter = driveId,
                        IsEnabled = true,
                    },

                    new NativeMenuItem("Save...")
                    {
                        Command = _viewModel.DiskDriveMenuViewModel.SaveCommand,
                        CommandParameter = driveId,
                        IsEnabled = _viewModel.DiskDriveMenuViewModel.SaveCommand.CanExecute(null),
                    },

                    new NativeMenuItemSeparator(),

                    new NativeMenuItem("View")
                    {
                        Command = _viewModel.DiskDriveMenuViewModel.ViewCommand,
                        CommandParameter = driveId,
                        IsEnabled = _viewModel.DiskDriveMenuViewModel.ViewCommand.CanExecute(null),
                    },

                    CreateDiskDriveEjectMenuItem(driveId),

                    new NativeMenuItemSeparator(),

                    CreateDiskDriveWriteProtectMenuItem(driveId),
                ]
            };

            _diskDrives[driveId] = diskItem;

            diskSubMenu.Items.Add(diskItem);
        }

        _diskDriveMenuItem.Menu = diskSubMenu;

        return _diskDriveMenuItem;
    }

    private NativeMenuItem CreateDebugMenu()
    {
        var debugItem = new NativeMenuItem("Debug");

        _debuggerBreakpointMenuItem = new NativeMenuItem("Breakpoints Enabled")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = _viewModel.ToggleBreakpointsCommand,
            IsChecked = _viewModel.BreakpointsEnabled,
        };

        var debugSubMenu = new NativeMenu
        {
            new NativeMenuItem("Debugger")
            {
                Command = _viewModel.ShowDebuggerViewCommand,
                Gesture = new KeyGesture(Key.F11)
            },

            _debuggerBreakpointMenuItem,

            new NativeMenuItemSeparator(),

            new NativeMenuItem("Memory View")
            {
                Command = _viewModel.ShowMemoryViewCommand,
            },
        };

        debugItem.Menu = debugSubMenu;

        return debugItem;
    }

    private NativeMenuItem CreateHelpMenu()
    {
        var helpItem = new NativeMenuItem("Help");

        var helpSubMenu = new NativeMenu
        {
            new NativeMenuItem("Keyboard")
            {
                Command = _viewModel.ShowKeyboardHelpViewCommand,
                Gesture = new KeyGesture(Key.F1),
            },

            new NativeMenuItemSeparator(),

            new NativeMenuItem("Log")
            {
                Command = _viewModel.ShowLogViewCommand,
            },
        };

        helpItem.Menu = helpSubMenu;

        return helpItem;
    }

    private NativeMenuItem CreateFavoritesMenu()
    {
        var favoritesItem = new NativeMenuItem("Favourites");

        var itemsMenu = new NativeMenu
        {
            new NativeMenuItem("Edit...")
            {
                Command = _viewModel.ShowFavoritesViewCommand,
            },
        };

        _viewModel.FavoritesViewModel.NativeFavoriteMenu = itemsMenu;
        _viewModel.FavoritesViewModel.RefreshMenu();

        favoritesItem.Menu = itemsMenu;

        return favoritesItem;
    }

    private NativeMenuItem CreateRecentMenuItem()
    {
        var recentItem = new NativeMenuItem("Recent...");

        var itemsMenu = new NativeMenu();
        itemsMenu.Opening += (_, _) => _viewModel.RecentFilesViewModel.Opening(itemsMenu);

        recentItem.Menu = itemsMenu;

        return recentItem;
    }

    private void CreateComputerTypeMenu()
    {
        var computers = new[]
        {
            new { Type = ComputerType.Spectrum16K, DisplayName = "ZX Spectrum 16" },
            new { Type = ComputerType.Spectrum48K, DisplayName = "ZX Spectrum 48" },
            new { Type = ComputerType.Spectrum128K, DisplayName = "ZX Spectrum 128" },
            new { Type = ComputerType.Timex2048, DisplayName = "Timex Computer 2048" },
        };

        foreach (var computer in computers)
        {
            _computerTypes[computer.Type] = new NativeMenuItem(computer.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.ChangeComputerTypeCommand,
                CommandParameter = computer.Type,
                IsChecked = _viewModel.ComputerType == computer.Type,
                IsEnabled = true,
            };
        }
    }

    private void CreateRomTypeMenu()
    {
        var roms = new[]
        {
            new { Type = RomType.Original, DisplayName = "Original" },
            new { Type = RomType.Pentagon128, DisplayName = "Pentagon (128 mode only)" },
            new { Type = RomType.GoshWonderful, DisplayName = "Gosh Wonderful" },
            new { Type = RomType.BusySoft, DisplayName = "Busy Soft v1.40" },
            new { Type = RomType.Harston, DisplayName = "J.G. Harston v0.77" },
            new { Type = RomType.HtrSuperBasic, DisplayName = "H.T.R. SuperBasic" },
            new { Type = RomType.PrettyBasic, DisplayName = "Pretty Basic" },
            new { Type = RomType.BbcBasic, DisplayName = "BBC Basic (128 mode only)" },
            new { Type = RomType.Retroleum, DisplayName = "Retroleum v1.73" },
            new { Type = RomType.BrendanAlford, DisplayName = "B. Alford v1.37" },
            new { Type = RomType.Custom, DisplayName = "Custom" },
        };

        foreach (var rom in roms)
        {
            _romTypes[rom.Type] = new NativeMenuItem(rom.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.ChangeRomCommand,
                CommandParameter = rom.Type,
                IsChecked = _viewModel.RomType == rom.Type,
                IsEnabled = true
            };
        }
    }

    private void CreateJoystickTypeMenu()
    {
        var joysticks = new[]
        {
            new { Type = JoystickType.None, DisplayName = "None" },
            new { Type = JoystickType.Kempston, DisplayName = "Kempston" },
            new { Type = JoystickType.Sinclair1, DisplayName = "Sinclair 1" },
            new { Type = JoystickType.Sinclair2, DisplayName = "Sinclair 2" },
            new { Type = JoystickType.Cursor, DisplayName = "Cursor" },
            new { Type = JoystickType.Fuller, DisplayName = "Fuller" },
        };

        foreach (var joystick in joysticks)
        {
            _joystickTypes[joystick.Type] = new NativeMenuItem(joystick.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.ChangeJoystickTypeCommand,
                CommandParameter = joystick.Type,
                IsChecked = _viewModel.JoystickType == joystick.Type,
                IsEnabled = true
            };
        }
    }

    private void CreateMouseTypeMenu()
    {
        var mice = new[]
        {
            new { Type = MouseType.None, DisplayName = "None" },
            new { Type = MouseType.Kempston, DisplayName = "Kempston" },
        };

        foreach (var mouse in mice)
        {
            _mouseTypes[mouse.Type] = new NativeMenuItem(mouse.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.ChangeMouseTypeCommand,
                CommandParameter = mouse.Type,
                IsChecked = _viewModel.MouseType == mouse.Type,
                IsEnabled = true
            };
        }
    }

    private void CreateSpeedOptionMenu()
    {
        var speeds = new[]
        {
            new { Value = 25, DisplayName = "25%" },
            new { Value = 50, DisplayName = "50%" },
            new { Value = 75, DisplayName = "75%" },
            new { Value = 100, DisplayName = "Normal" },
            new { Value = 125, DisplayName = "125%" },
            new { Value = 150, DisplayName = "150%" },
            new { Value = 200, DisplayName = "200%" },
            new { Value = 250, DisplayName = "250%" },
            new { Value = 300, DisplayName = "300%" },
            new { Value = 400, DisplayName = "400%" },
            new { Value = 500, DisplayName = "500%" },
            new { Value = -1, DisplayName = "Max" },
        };

        foreach (var speed in speeds)
        {
            _emulationSpeeds[speed.Value] = new NativeMenuItem(speed.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.SetEmulationSpeedCommand,
                CommandParameter = speed.Value,
                IsChecked = _viewModel.EmulationSpeed == speed.Value,
                IsEnabled = true
            };
        }
    }

    private void CreateClockMultiplierOptionMenu()
    {
        var clocks = new[]
        {
            new { Value = 1, DisplayName = "3.5 MHz" },
            new { Value = 2, DisplayName = "7 MHz" },
            new { Value = 4, DisplayName = "14 MHz" },
            new { Value = 8, DisplayName = "28 MHz" },
        };

        foreach (var clock in clocks)
        {
            _clockMultipliers[clock.Value] = new NativeMenuItem(clock.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.SetClockMultiplierCommand,
                CommandParameter = clock.Value,
                IsChecked = _viewModel.ClockMultiplier == clock.Value,
                IsEnabled = true
            };
        }
    }

    private void CreateBorderSizeMenu()
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
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.ChangeBorderSizeCommand,
                CommandParameter = border.Size,
                IsChecked = _viewModel.BorderSize == border.Size,
                IsEnabled = true
            };
        }
    }

    private void CreateScreenEffectMenu()
    {
        var effects = new[]
        {
            new { Effect = ScreenEffect.Blur, DisplayName = "Blur" },
            new { Effect = ScreenEffect.Crt, DisplayName = "CRT" },
        };

        foreach (var effect in effects)
        {
            _screenEffects[effect.Effect] = new NativeMenuItem(effect.DisplayName)
            {
                ToggleType = MenuItemToggleType.CheckBox,
                Command = _viewModel.ChangeScreenEffectCommand,
                CommandParameter = effect.Effect,
                IsChecked = _viewModel.ScreenEffect.HasFlag(effect.Effect),
                IsEnabled = true
            };
        }
    }

    private void CreateTapeLoadingSpeedMenu()
    {
        var speeds = new[]
        {
            new { Value = TapeSpeed.Normal, DisplayName = "Normal" },
            new { Value = TapeSpeed.Instant, DisplayName = "Instant" },
            new { Value = TapeSpeed.Accelerated, DisplayName = "Accelerated" },
        };

        foreach (var speed in speeds)
        {
            _tapeLoadingSpeeds[speed.Value] = new NativeMenuItem(speed.DisplayName)
            {
                ToggleType = MenuItemToggleType.Radio,
                Command = _viewModel.SetTapeLoadSpeedCommand,
                CommandParameter = speed.Value,
                IsChecked = _viewModel.TapeLoadSpeed == speed.Value,
                IsEnabled = true
            };
        }
    }

    private NativeMenuItem CreateMicrodriveEjectMenuItem(MicrodriveId driveId)
    {
        var menuItem = new NativeMenuItem(_viewModel.MicrodriveMenuViewModel.EjectCommandHeadings[driveId].Value)
        {
            Command = _viewModel.MicrodriveMenuViewModel.EjectCommand,
            CommandParameter = driveId,
            IsEnabled = _viewModel.MicrodriveMenuViewModel.EjectCommand.CanExecute(null),
        };

        _viewModel.MicrodriveMenuViewModel.EjectCommandHeadings[driveId].PropertyChanged += (_, _) =>
            menuItem.Header = _viewModel.MicrodriveMenuViewModel.EjectCommandHeadings[driveId].Value;

        return menuItem;
    }

    private NativeMenuItem CreateMicrodriveWriteProtectMenuItem(MicrodriveId driveId)
    {
        var menuItem = new NativeMenuItem("Write Protect")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = _viewModel.MicrodriveMenuViewModel.ToggleWriteProtectCommand,
            CommandParameter = driveId,
            IsEnabled = _viewModel.MicrodriveMenuViewModel.ToggleWriteProtectCommand.CanExecute(null),
            IsChecked = _viewModel.MicrodriveMenuViewModel.IsWriteProtected[driveId].Value,
        };

        _viewModel.MicrodriveMenuViewModel.IsWriteProtected[driveId].PropertyChanged += (_, _) =>
            menuItem.IsChecked = _viewModel.MicrodriveMenuViewModel.IsWriteProtected[driveId].Value;

        return menuItem;
    }

    private NativeMenuItem CreateDiskDriveEjectMenuItem(DriveId driveId)
    {
        var menuItem = new NativeMenuItem(_viewModel.DiskDriveMenuViewModel.EjectCommandHeadings[driveId].Value)
        {
            Command = _viewModel.DiskDriveMenuViewModel.EjectCommand,
            CommandParameter = driveId,
            IsEnabled = _viewModel.DiskDriveMenuViewModel.EjectCommand.CanExecute(null),
        };

        _viewModel.DiskDriveMenuViewModel.EjectCommandHeadings[driveId].PropertyChanged += (_, _) =>
            menuItem.Header = _viewModel.DiskDriveMenuViewModel.EjectCommandHeadings[driveId].Value;

        return menuItem;
    }

    private NativeMenuItem CreateDiskDriveWriteProtectMenuItem(DriveId driveId)
    {
        var menuItem = new NativeMenuItem("Write Protect")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Command = _viewModel.DiskDriveMenuViewModel.ToggleWriteProtectCommand,
            CommandParameter = driveId,
            IsEnabled = _viewModel.DiskDriveMenuViewModel.ToggleWriteProtectCommand.CanExecute(null),
            IsChecked = _viewModel.DiskDriveMenuViewModel.IsWriteProtected[driveId].Value,
        };

        _viewModel.DiskDriveMenuViewModel.IsWriteProtected[driveId].PropertyChanged += (_, _) =>
            menuItem.IsChecked = _viewModel.DiskDriveMenuViewModel.IsWriteProtected[driveId].Value;

        return menuItem;
    }
}