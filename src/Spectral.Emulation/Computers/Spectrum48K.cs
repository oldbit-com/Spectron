using System.Diagnostics;
using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Emulation.Tape;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulation.Computers;

// public sealed class Spectrum48K : Emulator
// {
//     private const float ClockMHz = 3.5f;
//     private const float InterruptFrequencyHz = ClockMHz * 1000000 / DefaultTimings.FrameTicks;
//
//     private readonly Beeper _beeper;
//     private readonly Z80 _z80;
//     private readonly TapeLoader _tapeLoader;
//     private readonly TapePlayer _tapePlayer;
//
//     internal Spectrum48K(
//         Memory48K memory,
//         Beeper beeper,
//         IContentionProvider contentionProvider) : base(memory, beeper, contentionProvider)
//     {
//         _beeper = beeper;
//
//
//         var screenRenderer = new ScreenRenderer(memory);
//
//         _z80 = new Z80(memory, contentionProvider);
//
//         _z80.BeforeFetch += BeforeInstructionFetch;
//
//         _tapePlayer = new TapePlayer(_z80.Clock);
//
//         _tapeLoader = new TapeLoader(_z80, memory, screenRenderer, _tapePlayer);
//     }
//
//     private void BeforeInstructionFetch(Word pc)
//     {
//         switch (pc)
//         {
//             case RomRoutines.LD_BYTES:
//             {
//                 if (!_tapePlayer.IsPlaying)
//                 {
//                     _tapePlayer.Start();
//                 }
//
//                 break;
//             }
//             case RomRoutines.SAVE_ETC:
//                 //
//                 break;
//         }
//     }
//
//     public void LoadFile(string fileName)
//     {
//         _tapeLoader.LoadFile(fileName);
//
//         // _memory.Write(RomRoutines.T_ADDR, 0xE1);
//         // _z80.Registers.PC = RomRoutines.SAVE_ETC;
//     }
// }