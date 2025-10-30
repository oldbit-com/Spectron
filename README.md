![Build and Test](https://github.com/oldbit-com/Spectron/actions/workflows/build.yml/badge.svg)

# Spectron ― ZX Spectrum 16/48/128 Emulator ☺
Here is my own ZX Spectrum emulator written in C# and Avalonia UI. It emulates classic ZX Spectrum 16K, 48K and 
128K computers.

It is quite accurate and stable, it can run most of the games and demos without any issues, load protected
tapes, etc.

It's a cross-platform emulator that runs on Windows, Linux and macOS. Developed on macOS, so generally well tested
on this platform. Needs some more testing on Linux, seems to be running fine on Windows.

This is purely my hobby project which I always wanted to do. It has been a lot of fun, and quite a challenge.
There are many other emulators available, but this one is my attempt. It is written all by hand, no AI generated code ☺.

ZX Spectrum was my first computer and I still love it. I am planning to keep this project alive since 
I have created it for my personal use to play games and demos. It is a lot of fun.

![Main Window](docs/default.png?raw=true "Main Window")

## Features
- [x] Emulates classic machines: ZX Spectrum 16K, 48K and 128K
- [x] Time Machine: rewind and continue from a given time point in the past
- [x] Emulator state is persisted and restored when the application is re-started
- [x] Supports SNA, SZX, Z80, TAP, TZX, MDR, TRD and SCL file formats (inside zip, too)
- [x] Supports POK trainer files
- [x] Accurate timings, including contented memory and IO
- [x] Floating bus support
- [x] Multicolor screen effects work, timings are quite accurate
- [x] ULA+ support
- [x] AY-3-8912 sound chip emulation
- [x] DivMMC emulation
- [x] Beta 128 Disk emulation
- [x] Microdrive emulation
- [x] Kempston mouse emulation
- [x] ZX Printer emulation
- [x] Tape viewer
- [x] Disk viewer
- [x] Adjustable emulator speed
- [x] Keyboard joystick emulation: Kempston, Sinclair, Cursor & Fuller.
- [x] Audio and video recording
- [x] Debugger
- [x] And more...

It uses several of my own libraries that I created for this project:

| Library                                                | Description                                  |
|--------------------------------------------------------|----------------------------------------------|
| [Z80](https://github.com/oldbit-com/Z80/tree/spectron) | Generic Z80 CPU emulator                     |
| [Files](https://github.com/oldbit-com/Spectron.Files)  | Handles TZX, Z80, SNA and other file formats |
| [Beep](https://github.com/oldbit-com/Beep)             | Basic audio player, cross-platform, native   |
| [Joypad](https://github.com/oldbit-com/Joypad)         | Gamepad handler, cross-platform, native      |

Solution consists of several projects:

| Project                | Description                                            |
|------------------------|--------------------------------------------------------|
| Spectron               | Avalonia based UI                                      |
| Spectron.Debugger      | Fully featured Code debugger, includes UI and controls |
| Spectron.Disassembly   | Simple Z80 disassembler, used by the debugger          |
| **Spectron.Emulation** | This is the core of the emulator, e.g. the main thing  |
| Spectron.Recorder      | Audio and Video recording helper                       |

## Quick demo
[![Spectron](https://img.youtube.com/vi/Oz70N0VY_2w/default.jpg)](https://youtu.be/Oz70N0VY_2w "Quick demo")

## Running the emulator
Requires .NET 9 to build and run the emulator.
Grab the latest code from the repository, build and run the emulator:

```shell
dotnet build -c Release
dotnet run --project ./src/Spectron   
```

### Command line options
Command lines allow overriding most of the default options and loading a specified file. Full list of available 
commands is [here](docs/CommandLine.md).

## Testing and compatibility
- [x] Passes floatspy v0.33 (RAMSOFT) floating bus test in both 48k and 128k mode
- [x] Passes HALT2INT v3 (Mark Woodmass) test in both 48k and 128k mode
- [x] Passes EIHALT (Mark Woodmass) test in both 48k and 128k mode
- [x] Passes Super HALT Invaders (Mark Woodmass) test
- [x] Passes btime (Jan Bobrowski) test

Test results can be found in the [Results](tests/Results) directory.
Test files can be found in the [Tests](tests/Files) directory.

## CPU emulation
I have created my own [Z80 CPU](https://github.com/oldbit-com/Z80/tree/spectron) emulator library for this project. 
CPU emulation is quite accurate and supports many undocumented instructions. 
Memory and IO contention is also supported.

## Session
When emulator is closed, it saves the current state of the emulator. The state will be restored when you start the emulator again.
This behaviour can be disabled in the settings.

## Tape loading
Tape loading is supported for **TAP** and **TZX** files (zip is ok). Three loading speeds are supported:
- **Normal** - loads the tape at normal speed, with border and audio effects,
- **Accelerated** - loads the tape running emulator at maximum speed,
- **Instant** - loads the tape instantly into memory. This will skip the tape loading animation and will load the tape instantly.
This mode will only work for files that are using standard ROM loaders.

## Tape saving
Tape saving is supported for **TAP** and **TZX** formats. Two saving speeds are supported:
- **Normal** - tape is saved at normal speed, with border and audio effects,
- **Instant** - tape is saved instantly reading the memory.

## Tape browser
Tape browser is a feature that allows you to browse the contents of currently loaded or saved tape. 
You can browse the blocks, their types and some basic block information. 
You can select a block and load it using standard `LOAD` command.

## Snapshots
Emulator supports saving and loading snapshots in **SNA**, **SZX**, **Z80** and custom format.
It is recommended to use a custom **\*.spectron** format when saving a snapshot. This format covers
most of the emulator settings.

## Screen
Multicolour screen effects are supported, as well as border effects. Border size can be adjusted.

## Floating bus
Floating bus is emulated and supported by both 48K and 128K modes. Only a handful of games require this feature.

## Audio
Standard beeper audio is supported, as well as AY audio (mono / stereo ABC or ACB mode).
AY is by default enabled in 48K mode, but can be disabled in the settings.

Audio playback is done using [Beep](https://github.com/oldbit-com/Beep) which I created for this project since I couldn't find any simple 
cross-platform audio player that would suit my needs.

## Joystick and Gamepad
Joystick emulation is supported for Kempston, Sinclair, Cursor and Fuller joysticks. External gamepads and joysticks 
are supported as well. But this has been only tested with few controllers I have. Controller buttons can be mapped to 
joystick or keyboard keys. Standard keyboard can also be used as a joystick, arrow keys for directions and space for fire.

> [!NOTE]
> Not all controllers may work, and compatibility depends on the platform. Experimental feature.
## ULA+ support
ULA+ mode is supported and can be enabled in the emulator settings.

## DivMMC
DivMMC emulation is supported and can be enabled in the emulator settings.
It is based on [esxDOS 0.8.9](https://esxdos.org/). You will need to use a disk image containing esxDOS files.
Sample disk images can be found [here](https://1drv.ms/u/c/7fd2cf29d9a4c5e1/EdULvNTWM7VLiU1_6GeGDcgBacxN6TanwmMCle2Hz8OBhg?e=3CSLB0).

Two SD card images are supported. Write changes can be persisted.

You can use [RTC.SYS](src/Spectron.Emulation/Devices/DivMmc/RTC) file to enable RTC support. This is not required, but it will enable DivMMC to use current 
date and time.

## Microdrives
Emulation of microdrives is supported and can be enabled in the emulator settings. 
Up to eight drives can be emulated, MDR image files can be loaded/saved. No other features of Interface 1 are emulated
currently.

## Beta 128 (TR-DOS)
Emulation of Beta 128 Disk Interface is supported and can be enabled in the emulator settings.
Up to four drives can be emulated, MDR and SCL image files can be loaded/saved. ROM version 5.03 is 
used.

To invoke TR-DOS, enter `RANDOMIZE USR 15616` from `BASIC`.

## Time Machine
Time Machine is a feature that allows you to go back in time and continue from given time point in the past.
The interval and the number of time points can be adjusted in the settings.

## Video and Audio recording

Audio and video recording is supported in the emulator. This is experimental feature and may not work on all platforms.
You can pause emulator during the recording, however changing emulator settings during the recording may cause
unexpected results.

### Audio
Audio can be recorded to a file in **WAV** format. The format is PCM 16-bit, 44,100 Hz, mono or stereo depending on the 
current AY mode. No external dependencies are required for audio recording.

### Video
Video recording requires **[FFmpeg](https://www.ffmpeg.org)** to be installed on your system.

Video is generated as **MP4** using **H.264** codec at **50 FPS**. Some video rendering options like scaling, border size
can be adjusted in the emulator settings. Raw frame buffer data is used internally during the recording.

Processing of the recorded data starts after the recording is stopped, and it can take some time. This is 
done in the background by converting static frames to a video stream with audio, leveraging FFmpeg.

## Debugger
Debugger is available in the emulator. It is a simple debugger that allows you to inspect the CPU registers, 
memory and disassembly. You can step through the code, set breakpoints.
More information can be found [here](docs/Debugger.md).

### Resources
- [Avalonia UI](https://avaloniaui.net/)
- [FFmpeg wrapper](https://github.com/rosenbjerg/FFMpegCore)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [Material Icons](https://github.com/SKProCH/Material.Icons)
- [ZX Spectrum Font](https://github.com/comptic/zx-spectrum-font)
- [Hack Font](https://sourcefoundry.org/hack/)
- [VT220 Font](https://github.com/svofski/glasstty/blob/master/Glass_TTY_VT220.ttf)