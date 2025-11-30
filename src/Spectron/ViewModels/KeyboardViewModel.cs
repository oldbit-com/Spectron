using System.Threading.Tasks;
using Avalonia;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Input;

namespace OldBit.Spectron.ViewModels;

public class KeyboardViewModel
{
   private const int Distance = 83;
   private const int KeyWidth = 61;

   private readonly int[] _leftOffsets = [40, 80, 104, 146];
   private readonly (int Min, int Max)[] _rowVerticalBounds = [
      (17, 103),     // 1..0
      (114, 186),    // Q..P
      (198, 270),    // A..Enter
      (281, 353)     // Caps..Space
   ];

   public required KeyboardHandler KeyboardHandler { get; init; }

   public void DoubleClick(Point point)
   {
      var row = GetRow(point);

      if (row == -1)
      {
         return;
      }

      var (key, x, y) = GetKey(row, point);

      var spectrumKey = (row, key) switch
      {
         (0, 0) => SpectrumKey.D1,
         (0, 1) => SpectrumKey.D2,
         (0, 2) => SpectrumKey.D3,
         (0, 3) => SpectrumKey.D4,
         (0, 4) => SpectrumKey.D5,
         (0, 5) => SpectrumKey.D6,
         (0, 6) => SpectrumKey.D7,
         (0, 7) => SpectrumKey.D8,
         (0, 8) => SpectrumKey.D9,
         (0, 9) => SpectrumKey.D0,

         (1, 0) => SpectrumKey.Q,
         (1, 1) => SpectrumKey.W,
         (1, 2) => SpectrumKey.E,
         (1, 3) => SpectrumKey.R,
         (1, 4) => SpectrumKey.T,
         (1, 5) => SpectrumKey.Y,
         (1, 6) => SpectrumKey.U,
         (1, 7) => SpectrumKey.I,
         (1, 8) => SpectrumKey.O,
         (1, 9) => SpectrumKey.P,

         (2, 0) => SpectrumKey.A,
         (2, 1) => SpectrumKey.S,
         (2, 2) => SpectrumKey.D,
         (2, 3) => SpectrumKey.F,
         (2, 4) => SpectrumKey.G,
         (2, 5) => SpectrumKey.H,
         (2, 6) => SpectrumKey.J,
         (2, 7) => SpectrumKey.K,
         (2, 8) => SpectrumKey.L,
         (2, 9) => SpectrumKey.Enter,

         (3, 0) => SpectrumKey.Z,
         (3, 1) => SpectrumKey.X,
         (3, 2) => SpectrumKey.C,
         (3, 3) => SpectrumKey.V,
         (3, 4) => SpectrumKey.B,
         (3, 5) => SpectrumKey.N,
         (3, 6) => SpectrumKey.M,
         (3, 7) => SpectrumKey.SymbolShift,
         (3, 8) => SpectrumKey.CapsShift,
         (3, 9) => SpectrumKey.Space,

         _ => SpectrumKey.None
      };

      if (spectrumKey == SpectrumKey.None)
      {
         return;
      }

      KeyboardHandler.SimulatePressKey(spectrumKey);
      Task.Delay(20).ContinueWith(_ => KeyboardHandler.SimulateReleaseKey(spectrumKey));
   }

   private int GetRow(Point point)
   {
      for (var row = 0; row < _rowVerticalBounds.Length; row++)
      {
         if (point.Y >= _rowVerticalBounds[row].Min && point.Y <= _rowVerticalBounds[row].Max)
         {
            return row;
         }
      }

      return -1;
   }

   private (int Key, double X, double Y) GetKey(int row, Point point)
   {
      var key = -1;

      if (row == 3)
      {
         key = point.X switch
         {
            >= 39 and <= 124 => 8,
            >= 810 and <= 910 => 9,
            _ => key
         };
      }

      var pos = (point.X - _leftOffsets[row]) / Distance;
      var mod = (point.X - _leftOffsets[row]) % Distance;

      if (mod is >= 0 and <= KeyWidth)
      {
         key = (int)pos;
      }

      var y = point.Y - _rowVerticalBounds[row].Min;
      var x = point.X - _leftOffsets[row] - key * Distance;

      return (key, x, y);
   }
}