# Spectron (ZX Spectrum emulator)
My ZX Spectrum emulator written in C# and Avalonia as UI. 
It is a cross platform emulator that runs on Windows, Linux and MacOS.

This is my hobby project which I always wanted to do it and it has been a lot of fun and a challenge, too.
Something different from what I do in my day job as software developer. ZX Spectrum was my first computer and I loved it.

UI is written using excellent [Avalonia](https://avaloniaui.net) framework.

Z80 CPU is written in C# from scratch and it is quite accurate.

## Features
- [x] Emulates classic ZX Spectrum 16K, 48K and 128K (toastrack)
- [x] Time Machine: rewind and replay your games
- [x] Resume: save and restore emulator state
- [x] Supports SNA, SZX, Z80, TAP and TZX files
- [x] Quite accurate timings, including contented memory and IO
- [x] Multicolor screen effects in games like Uridium
- [x] ULA+ support (multicolor)
- [x] Floating bus support
- [x] Adjustable emulator speed
- [x] Cursor keys for joystick emulation: Kempston, Sinclair, Cursor & Fuller.

### Time Machine
Time Machine is a feature that allows you to rewind and replay your games. Internally, every given time interval, 
emulator state is saved and when you rewind, emulator state is restored to that point in time. It is like a time machine.

