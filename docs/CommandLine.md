### List of available command line arguments

Command line arguments can override any default option. If specified they take priority over saved preferences
or session settings.

```
Options:
  -f, --file <file>                                                                            Specifies the file to load. This can be any supported file type: TAP | TZX | Z80 | SNA | 
                                                                                               SZX | POK | ZIP
  -c, --computer <Spectrum128K|Spectrum16K|Spectrum48K|Timex2048>                              Specifies the computer to emulate
  -j, --joystick <Cursor|Fuller|Kempston|None|Sinclair1|Sinclair2>                             Specifies the emulated joystick type
  -m, --mouse <Kempston|None>                                                                  Specifies the emulated mouse type
  -t, --tape-load-speed <Accelerated|Instant|Normal>                                           Specifies the tape loading speed
  -b, --border <Full|Large|Medium|None|Small>                                                  Specifies the border size
  -r, --rom                                                                                    Specifies the ROM to load
  <BbcBasic|BrendanAlford|BusySoft|Custom|GoshWonderful|Harston|HtrSuperBasic|Original|Pentag
  on128|PrettyBasic|Retroleum>
  -rf, --rom-file <rom-file>                                                                   Specifies the custom ROM file
  --time-machine                                                                               Enables Time Machine
  --no-time-machine                                                                            Disables Time Machine
  --resume                                                                                     Resume emulator state assuming there is a previously saved state
  --no-resume                                                                                  Do not resume emulator state
  --mute                                                                                       Mutes the audio playback
  --ay                                                                                         Enables AY sound emulation
  --no-ay                                                                                      Disables AY sound emulation
  --ay-mode <Mono|StereoABC|StereoACB>                                                         Specifies AY mono or stereo mode
  --ula-plus                                                                                   Enables ULA+ emulation
  --no-ula-plus                                                                                Disables ULA+ emulation
  --divmmc                                                                                     Enables divMMC emulation, --divmmc-image is required
  --no-divmmc                                                                                  Disables divMMC emulation
  --divmmc-image <divmmc-image>                                                                Specifies the SD card image to use with divMMC
  --divmmc-readonly                                                                            Specifies the SD card image is readonly, SD card writes will be cached in-memory only
  --divmmc-writable                                                                            Specifies the SD card image is writable, SD card writes will persisted
  --interface1                                                                                 Enables Interface 1 emulation
  --no-interface1                                                                              Disables Interface 1 emulation
  --interface1-rom <V1|V2>                                                                     Specifies ROM version to use for Interface 1 emulation
  --zx-printer                                                                                 Enables ZX Printer emulation
  --no-zx-printer                                                                              Disables ZX Printer emulation
  --theme <Dark|Light>                                                                         Specifies the application theme
  -?, -h, --help                                                                               Show help and usage information
  --version                                                                                    Show version information
```
