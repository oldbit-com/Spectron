# Spectron (ZX Spectrum emulator)
My own ZX Spectrum emulator written in C# and Avalonia UI. It emulates ZX Spectrum 16K, 48K and classic 128K computers.

This is work in progress, I've been improving it quite a lot recently. However it is stable and usable. 
It can run most of the games and demos without any problems.

It's a cross-platform emulator that runs on Windows, Linux and macOS. Developed on macOS, so mostly tested
on this platform. Needs more testing and feedback on Linux.

This is my hobby project which I always wanted to do. It has been a lot of fun, and quite challenging, too.
ZX Spectrum was my first computer and I love it. I am planning to keep it alive since I created it for my own 
use and I am using it to play games and demos.

It uses several of my own libraries that I created for this project:
- [Z80 CPU emulator](https://github.com/oldbit-com/Z80/tree/spectron)
- [File format handling](https://github.com/oldbit-com/Spectron.Files)
- [Audio player](https://github.com/oldbit-com/Beep)
- [Gamepad support](https://github.com/oldbit-com/Joypad)

# Features
- [x] Emulates classic ZX Spectrum 16K, 48K and 128K
- [x] Time Machine: go back in time to continue from there
- [x] Remembers the last state of the emulator
- [x] Supports SNA, SZX, Z80, TAP and TZX file formats (zip is fine, too)
- [x] Tape content browser
- [x] Accurate timings, including contented memory and IO
- [x] Floating bus support
- [x] Multicolor screen effects in games and demos
- [x] ULA+ support
- [x] AY-3-8912 sound chip emulation
- [x] Adjustable emulator speed
- [x] Debugger
- [x] Keyboard joystick emulation: Kempston, Sinclair, Cursor & Fuller.
- [x] Audio and video recording
- [x] And more features in progress...

# Running the emulator
Requires .NET 9 in order to build and run the emulator.
Grab the latest code from the repository, build and run the emulator:

```shell
dotnet build -c Release
dotnet run --project ./src/Spectron   
```

## CPU emulation
I have created my own [Z80 CPU](https://github.com/oldbit-com/Z80/tree/spectron) emulator for this project. 
CPU emulation is quite accurate and supports many undocumented instructions. Memory and IO contention is also supported.

## Tape loading
Tape loading is supported for **TAP** and **TZX** files (zip is ok). Three loading speeds are supported:
- **Normal** - loads the tape at normal speed, with border and audio effects
- **Accelerated** - loads the tape running emulator at maximum speed
- **Instant** - loads the tape instantly into memory. This will skip the tape loading animation and will load the tape instantly.

> [!NOTE]
> Instant mode is not supported for all tapes, especially for those that use custom loaders. In this case try normal or accelerated mode.

## Tape saving
Tape saving is supported for **TAP** and **TZX** formats. Two saving speeds are supported:
- **Normal** - tape is saved at normal speed, with border and audio effects.
- **Instant** - tape is saved instantly reading the memory.

> [!NOTE]
> Saved tape contents can be accessed in the tape browser.

## Tape browser
Tape browser is a feature that allows you to browse the contents of the tape. 
You can browse the blocks, their types and lengths. You can select a block and load it using standard LOAD command.

## Snapshots
Emulator supports saving and loading snapshots in **SNA**, **SZX** and **Z80** formats (can be inside zip file).
It is recommended to use **SZX** format when saving a snapshot since it is the most robust and supports all features of the emulator.

## Screen
Multicolor screen effects are supported, as well as border effects. Border size can be adjusted.

## Floating bus
Floating bus is emulated and supported by both 48K and 128K modes. Only a handful of games require this feature.

## Audio
Standard beeper audio is supported, as well as AY audio (mono / stereo ABC or ACB mode).
AY is by default enabled in 48K mode, but can be disabled in the settings..

Audio playback is done using [Beep](https://github.com/oldbit-com/Beep) which I created for this project since I couldn't find any simple 
cross-platform audio player that would suit my needs.

## Joystick and Gamepad
Joystick emulation is supported for Kempston, Sinclair, Cursor and Fuller joysticks. External gamepads and joysticks 
are supported as well. But this has been only tested with few controllers I have. Controller buttons can be mapped to 
joystick or keyboard keys. Standard keyboard can also be used as a joystick, arrow keys for directions and space for fire.

> [!NOTE]
> Not all controllers may work, and compatibility depends on the platform. Experimental feature.
## ULA+ support
ULA+ mode is supported and can be enabled or disabled in the emulator settings.

## Time Machine
Time Machine is a feature that allows you to go back in time and continue from given time point in the past.
The interval and the number of time points can be adjusted in the settings.

## Video and Audio recording

Audio and video recording is supported in the emulator. This is experimental feature and may not work on all platforms.
You can pause emulator during the recording, but changing emulator settings during the recording may cause
unexpected results.

### Audio
Audio can be recorded to a file in **WAV** format. The format is PCM 16-bit, 44100 Hz, mono or stereo depending on the 
current AY mode. No external dependencies are required for audio recording.

### Video
Video recording requires **[FFmpeg](https://www.ffmpeg.org)** to be installed on your system.

Video is generated as **MP4** using **H.264** codec at **50 FPS**. Some video rendering options like scaling, border size
can be adjusted in the emulator settings. Raw frame buffer data is used internally during the recording.

Processing of the recorded data starts after the recording is stopped and it can take some time. This is 
done in a background by converting static frames to a video stream with audio, leveraging FFmpeg.

## Debugger
Debugger is available in the emulator. It is a simple debugger that allows you to inspect the CPU registers, 
memory and disassembly. You can step through the code, set breakpoints. This is still work in progress.

### Resources
- [Avalonia UI](https://avaloniaui.net/)
- [FFmpeg wrapper](https://github.com/rosenbjerg/FFMpegCore)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [ZX Spectrum Font](https://github.com/comptic/zx-spectrum-font)
- [Hack Font](https://sourcefoundry.org/hack/)
- [VT220 Font](https://github.com/svofski/glasstty/blob/master/Glass_TTY_VT220.ttf)

