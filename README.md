# Spectron (ZX Spectrum emulator)
My own ZX Spectrum emulator written in C# and Avalonia UI.

Still work in progress, but quite stable and usable. It can run most of the games and demos.

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

### Time Machine
Time Machine is a feature that allows you to go back in time and continue from given time point in the past.
The interval and the number of time points can be adjusted in the settings.

