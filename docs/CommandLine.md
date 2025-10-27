### List of available command line arguments

Command line arguments can override any default option. If specified they take priority over saved preferences
or session settings.

```
Options:
  -?, -h, --help                                                                      Show help and usage information
  --version                                                                           Show version information
  -f, --file                                                                          Specifies the file to load. This can be any supported file type: TAP | TZX | Z80 | SNA | SZX | 
                                                                                      POK | ZIP
  -ts, --tape-load-speed <Accelerated|Instant|Normal>                                 Specifies the tape loading speed
  -c, --computer <Spectrum128K|Spectrum16K|Spectrum48K>                               Specifies the computer to emulate
  -r, --rom <BrendanAlford|BusySoft|Custom|GoshWonderful|Harston|Original|Retroleum>  Specifies the ROM to load
  -rf, --rom-file                                                                     Specifies the custom ROM file
  -j, --joystick <Cursor|Fuller|Kempston|None|Sinclair1|Sinclair2>                    Specifies the emulated joystick type
  -m, --mouse <Kempston|None>                                                         Specifies the emulated mouse type
  -t, --theme <Dark|Light>                                                            Specifies the application theme
  -b, --border <Full|Large|Medium|None|Small>                                         Specifies the border size
  -m, --mute                                                                          Mutes the audio playback [default: False]
  --ay                                                                                Enables AY sound emulation
  --no-ay                                                                             Disables AY sound emulation
  --ay-mode <Mono|StereoABC|StereoACB>                                                Specifies AY mono or stereo mode
  --zx-printer                                                                        Enables ZX Printer emulation
  --no-zx-printer                                                                     Disables ZX Printer emulation
  --ula-plus                                                                          Enables ULA+ emulation
  --no-ula-plus                                                                       Disables ULA+ emulation
  --divmmc                                                                            Enables divMMC emulation, --divmmc-image is required
  --no-divmmc                                                                         Disables divMMC emulation
  --divmmc-image                                                                      Specifies the SD card image to use with divMMC
  --divmmc-readonly                                                                   Specifies the SD card image is readonly, SD card writes will be cached in-memory only
  --divmmc-writable                                                                   Specifies the SD card image is writable, SD card writes will persisted
  --time-machine                                                                      Enables Time Machine
  --no-time-machine                                                                   Disables Time Machine
  --resume                                                                            Resume emulator state assuming there is a previously saved state
  --no-resume                                                                         Do not resume emulator state
  --if1, --interface1                                                                 Enables Interface 1 emulation
  --no-if1, --no-interface1                                                           Disables Interface 1 emulation
  --interface1-rom <V1|V2>                                                            Specifies ROM version to use for Interface 1 emulation                                                                       Do not resume emulator state
```
