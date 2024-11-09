# Spectron (ZX Spectrum emulator)
My ZX Spectrum emulator written in C# and Avalonia UI. For my own use.

Still work in progress, but quite stable and usable. It can run most of the games and demos.

It's a cross platform emulator that runs on Windows, Linux and MacOS. Developed on MacOS, so mostly tested
on this platform.

This is my hobby project which I always wanted to do. It has been a lot of fun and a challenge, too.

Something different from what I do in my day job as software developer. ZX Spectrum was my first computer and I loved it.

Z80 CPU emulator and audio player are written from scratch as a separate library and can be used in other projects.

## Features
- [x] Emulates classic ZX Spectrum 16K, 48K and 128K (toastrack)
- [x] Time Machine: rewind and replay your games
- [x] Resume: save and restore emulator state
- [x] Supports SNA, SZX, Z80, TAP and TZX files
- [x] Quite accurate timings, including contented memory and IO
- [x] Multicolor screen effects in games like Uridium
- [x] ULA+ support (multicolor)
- [x] AY-3-8912 sound chip emulation
- [x] Floating bus support
- [x] Adjustable emulator speed
- [x] Cursor keys for joystick emulation: Kempston, Sinclair, Cursor & Fuller.
- [x] And more features...

### Time Machine
Time Machine is a feature that allows you to rewind and replay your games. Internally, every given time interval, 
emulator state is saved and when you rewind, emulator state is restored to that point in time. It is like a time machine.

