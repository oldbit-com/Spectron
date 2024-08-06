// using OldBit.Spectral.Emulation.Screen;
// using OldBit.Z80Cpu.Contention;
//
// namespace OldBit.Spectral.Emulation.Devices.Memory;
//
// public class ContentionProvider128K : IContentionProvider
// {
//     public int GetMemoryContention(int ticks, ushort address)
//     {
//         throw new NotImplementedException();
//     }
//
//     public int GetPortContention(int ticks, ushort port)
//     {
//         throw new NotImplementedException();
//     }
//
//     private static int[] BuildContentionTable()
//     {
//         var contentionTable = new int[DefaultTimings.FirstPixelTick + DefaultSizes.ContentHeight * DefaultTimings.LineTicks];
//
//         for (var line = 0; line < DefaultSizes.ContentHeight; line++)
//         {
//             var startLineState = DefaultTimings.FirstPixelTick + line * DefaultTimings.LineTicks;
//
//             for (var i = 0; i < 128; i += ContentionPattern.Length)
//             {
//                 for (var delay = 0; delay < ContentionPattern.Length; delay++)
//                 {
//                     contentionTable[startLineState + i + delay] = ContentionPattern[delay];
//                 }
//             }
//         }
//
//         return contentionTable;
//     }
// }