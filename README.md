# Spectron (ZX Spectrum emulator)
My own ZX Spectrum emulator written in C# and Avalonia UI. It emulates ZX Spectrum 16K, 48K and 128K (toastrack).

It is work in progress, but quite stable and usable. It can run most of the games and demos.

It's a cross-platform emulator that runs on Windows, Linux and macOS. Developed on macOS, so mostly tested
on this platform.

This is my hobby project which I always wanted to do. It has been a lot of fun and a challenge, too.
Something different from what I do in my day job as software developer. ZX Spectrum was my first computer and I still love it.
I am planning to keep it alive since I created it for my own use and I am using it to play games and demos.

I have created separate libraries for:
- [Z80 CPU emulator](https://github.com/oldbit-com/Z80.Spectron)
- [audio player](https://github.com/oldbit-com/Beep)
- [gamepad support](https://github.com/oldbit-com/Joypad)

## Features
- [x] Emulates classic ZX Spectrum 16K, 48K and 128K (toastrack)
- [x] Time Machine: rewind and replay your games
- [x] Save and restore emulator state on startup
- [x] Supports SNA, SZX, Z80, TAP and TZX file formats (can be zipped)
- [x] Quite accurate timings, including contented memory and IO
- [x] Multicolor screen effects in games like Uridium
- [x] ULA+ support
- [x] AY-3-8912 sound chip emulation
- [x] Floating bus support
- [x] Adjustable emulator speed
- [x] Cursor keys for joystick emulation: Kempston, Sinclair, Cursor & Fuller.
- [x] And more features...

### Tape loading
Tape loading is supported for **TAP** and **TZX** files (can be inside zip file). Three loading speeds are supported:
#### Normal
Loads the tape at normal speed, with border and audio effects.
#### Accelerated
Loads the tape running emulator at maximum speed
#### Instant
Loads the tape instantly into memory. This will skip the tape loading animation and will load the tape instantly.

> [!NOTE]
> Instant mode is not supported for all tapes, especially for those that use custom loaders. In this case try normal or accelerated mode.

### Tape saving
Tape saving is supported for **TAP** and **TZX** formats. Two saving speeds are supported:
#### Normal
Tape is saved at normal speed, with border and audio effects.
### Instant
Tape is saved instantly. This will skip the tape saving animation and will save the tape instantly.

> [!NOTE]
> Saved tape contents can be accessed in the tape browser.

### Snapshots
Emulator supports saving and loading snapshots in **SNA**, **SZX** and **Z80** formats (can be inside zip file).
It is recommended to use **SZX** format when saving snapshot since it is the most robust and supports all features of the emulator.

### Screen
Multicolor screen effects are supported, as well as border effects.

### Audio
Standard beeper audio is supported, as well as AY audio (mono or stereo ABC or ACB mode).
Additionally AY can be enabled in 48K mode, not only in 128K mode.

Audio playback is done using [Beep](https://github.com/oldbit-com/Beep) which I created for this 
project since I couldn't find any simple cross-platform audio player that would suit my needs.

### Joystick and Gamepad
Joystick emulation is supported for Kempston, Sinclair, Cursor and Fuller joysticks. External gamepads and joysticks 
are supported as well. But this has been only tested with few controllers I have. Again I created my own library for this:
[Joypad](https://github.com/oldbit-com/Joypad). Controller buttons can be mapped to joystick or keyboard keys.

> [!NOTE]
> Not all controllers may work, and compatibility depends on the platform and the controller. I have limited
> number of controllers to test with.

### ULA+ support
ULA+ mode is supported and can be enabled or disabled in the emulator settings.

### Time Machine
Time Machine is a feature that allows you to go back in time and continue from given time point in the past.
The interval and the number of time points can be adjusted in the settings.

### Debugger
Debugger is available in the emulator. It is a simple debugger that allows you to inspect the CPU registers, 
memory and disassembly. You can step through the code, set breakpoints.

