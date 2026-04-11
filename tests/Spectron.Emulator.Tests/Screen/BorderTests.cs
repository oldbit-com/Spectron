using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Screen;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Screen;

public class BorderTests
{
    private readonly List<BorderTick> _borderTicks =
        Border.BuildBorderTickRanges(Hardware.Spectrum48K.RetraceTicks, Hardware.Spectrum48K.BorderTop);

    private readonly List<BorderTick> _hiResBorderTicks =
        Border.BuildBorderTickRanges(Hardware.Timex2048.RetraceTicks, Hardware.Timex2048.BorderTop, ScreenSize.ContentWidth * 2);

    [Theory]
    [InlineData(0, 0, 151, 48)]                 // first top border line
    [InlineData(1, 200, 375, 352)]
    [InlineData(2, 424, 599, 704)]
    [InlineData(3, 648, 823, 1056)]
    [InlineData(63, 14088, 14263, 22176)]
    [InlineData(64, 14312, 14335, 22528)]       // first screen line left border
    [InlineData(65, 14464, 14487, 22832)]       // first screen line right border
    [InlineData(66, 14536, 14559, 22880)]
    [InlineData(67, 14688, 14711, 23184)]
    [InlineData(446, 57096, 57119, 89760)]      // last screen line left border
    [InlineData(447, 57248, 57271, 90064)]      // last screen line right border
    [InlineData(448, 57320, 57495, 90112)]      // first bottom border line
    [InlineData(449, 57544, 57719, 90464)]
    [InlineData(503, 69640, 69815, 109472)]     // last bottom border line
    public void BorderTicksTable_ShouldHaveCorrectRange(int index, int startTick, int endTick, int startPixel)
    {
        _borderTicks[index].StartTick.ShouldBe(startTick);
        _borderTicks[index].EndTick.ShouldBe(endTick);
        _borderTicks[index].StartPixel.ShouldBe(startPixel);
        _borderTicks[index].Shift.ShouldBe(1);
    }

    [Theory]
    [InlineData(0, 0, 151, 96, 2)]
    [InlineData(1, 200, 375, 704, 2)]
    [InlineData(2, 424, 599, 1408, 2)]
    [InlineData(3, 648, 823, 2112, 2)]
    [InlineData(4, 872, 1047, 2816, 2)]
    [InlineData(191, 28576, 28599, 90016, 2)]
    [InlineData(501, 69192, 69367, 217536, 2)]
    [InlineData(502, 69416, 69591, 218240, 2)]
    [InlineData(503, 69640, 69815, 218944, 2)]
    public void BorderTicksTable_ShouldHaveCorrectHiResRanges(
        int index,
        int startTick,
        int endTick,
        int startPixel,
        int shift)
    {
        _hiResBorderTicks[index].StartTick.ShouldBe(startTick);
        _hiResBorderTicks[index].EndTick.ShouldBe(endTick);
        _hiResBorderTicks[index].StartPixel.ShouldBe(startPixel);
        _hiResBorderTicks[index].Shift.ShouldBe(shift);
    }

    [Fact]
    public void BorderRenderer_ShouldSetBorderBlue()
    {
        var screenBuffer = new FrameBuffer();
        var borderRenderer = new Border(Hardware.Spectrum48K, screenBuffer);
        borderRenderer.Update(SpectrumPalette.Blue);

        borderRenderer.Update(SpectrumPalette.Blue, 69888);

        BorderShouldHaveColor(SpectrumPalette.Blue, screenBuffer.Pixels[..109823]);
    }

    [Fact]
    public void BorderRenderer_ShouldSetBorderRed()
    {
        var random = new Random(69888);

        var screenBuffer = new FrameBuffer();
        var borderRenderer = new Border(Hardware.Spectrum48K, screenBuffer);
        borderRenderer.Update(SpectrumPalette.Red);

        var ticks = 0;
        while (ticks < 69888)
        {
            ticks += random.Next(4, 1000);
            borderRenderer.Update(SpectrumPalette.Red, ticks);
        }

        BorderShouldHaveColor(SpectrumPalette.Red, screenBuffer.Pixels[..109823]);
    }

    [Fact]
    public void BorderRenderer_ShouldMatchAquaplane()
    {
        var screenBuffer = new FrameBuffer();
        var borderRenderer = new Border(Hardware.Spectrum48K, screenBuffer);

        borderRenderer.Update(SpectrumPalette.Cyan, 1);
        borderRenderer.Update(SpectrumPalette.Cyan, 145);
        borderRenderer.Update(SpectrumPalette.Blue, 25013);
        borderRenderer.Update(SpectrumPalette.Blue, 69888);

        screenBuffer.Pixels[..50].ShouldAllBe(c => c == SpectrumPalette.White);
        screenBuffer.Pixels[50..351].ShouldAllBe(c => c == SpectrumPalette.Cyan);
    }

    [Fact]
    public void BorderRenderer_ShouldFillFullTopAndBottomBordersInTimexHiRes()
    {
        var screenBuffer = new FrameBuffer();
        screenBuffer.ChangeScreenMode(ScreenMode.TimexHiRes);

        var borderRenderer = new Border(Hardware.Timex2048, screenBuffer);
        borderRenderer.ChangeScreenMode(ScreenMode.TimexHiRes);
        borderRenderer.Update(SpectrumPalette.Red);

        borderRenderer.Update(SpectrumPalette.Red, Hardware.Timex2048.TicksPerFrame);

        HiResBorderShouldHaveColor(SpectrumPalette.Red, screenBuffer.Pixels);
    }

    private static void BorderShouldHaveColor(Color color, Color[] screenBuffer)
    {
        screenBuffer[..47].ShouldAllBe(c => c == SpectrumPalette.White);
        screenBuffer[48..22576].ShouldAllBe(c => c == color);

        for (var i = 0; i < 192; i++)
        {
            screenBuffer.Skip(22528 + (i * 352)).Take(48).ShouldAllBe(c => c == color);
            screenBuffer.Skip(22576 + (i * 352)).Take(256).ShouldAllBe(c => c == SpectrumPalette.White);
            screenBuffer.Skip(22832 + (i * 352)).Take(48).ShouldAllBe(c => c == color);
        }

        screenBuffer[90112..].ShouldAllBe(c => c == color);
    }

    private static void HiResBorderShouldHaveColor(Color color, Color[] screenBuffer)
    {
        const int width = (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight) * 2;
        const int topPixels = ScreenSize.BorderTop * width;
        const int bottomStart = topPixels + ScreenSize.ContentHeight * width;

        screenBuffer[..95].ShouldAllBe(c => c == SpectrumPalette.White);
        screenBuffer[96..topPixels].ShouldAllBe(c => c == color);

        for (var i = 0; i < ScreenSize.ContentHeight; i++)
        {
            screenBuffer
                .Skip(topPixels + i * width)
                .Take(ScreenSize.BorderLeft * 2)
                .ShouldAllBe(c => c == color);

            screenBuffer
                .Skip(topPixels + i * width + ScreenSize.BorderLeft * 2)
                .Take(ScreenSize.ContentWidth * 2)
                .ShouldAllBe(c => c == SpectrumPalette.White);

            screenBuffer
                .Skip(topPixels + i * width + ScreenSize.BorderLeft * 2 + ScreenSize.ContentWidth * 2)
                .Take(ScreenSize.BorderRight)
                .ShouldAllBe(c => c == color);
        }

        screenBuffer[bottomStart..(width * (ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom))]
            .ShouldAllBe(c => c == color);
    }
}
