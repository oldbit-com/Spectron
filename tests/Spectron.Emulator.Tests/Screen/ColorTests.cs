using System.Runtime.InteropServices;
using OldBit.Spectron.Emulation.Screen;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Screen;

public class ColorTests
{
    [Fact]
    public void ColorStruct_SizeShouldBe4Bytes()
    {
        var size = Marshal.SizeOf<Color>();

        size.ShouldBe(4);
    }

    [Fact]
    public void ColorStruct_SequenceShouldBeRgba8888()
    {
        const byte red = 1;
        const byte green = 2;
        const byte blue = 3;
        const byte alpha = 0xFF;

        var color = new Color(red, green, blue);

        unsafe
        {
            var ptr = &color;
            var value = *((uint*)ptr);

            value.ShouldBe(0xFF030201);

            ptr->Red.ShouldBe(red);
            ptr->Green.ShouldBe(green);
            ptr->Blue.ShouldBe(blue);
            ptr->Alpha.ShouldBe(alpha);
        }
    }
}