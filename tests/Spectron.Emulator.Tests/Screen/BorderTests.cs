using FluentAssertions;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.ZXSpectrum.Emulator.Tests.Screen;

public class BorderTests
{
    private readonly List<BorderTick> _borderTicks = Border.BuildBorderTickRanges();

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
        _borderTicks[index].StartTick.Should().Be(startTick);
        _borderTicks[index].EndTick.Should().Be(endTick);
        _borderTicks[index].StartPixel.Should().Be(startPixel);
    }

    [Fact]
    public void BorderRenderer_ShouldSetBorderBlue()
    {
        var screenBuffer = new FrameBuffer(SpectrumPalette.White);
        var borderRenderer = new Border(screenBuffer);
        borderRenderer.Update(SpectrumPalette.Blue);

        borderRenderer.Update(SpectrumPalette.Blue, 69888);

        BorderShouldHaveColor(SpectrumPalette.Blue, screenBuffer.Pixels);
    }

    [Fact]
    public void BorderRenderer_ShouldSetBorderRed()
    {
        var random = new Random(69888);

        var screenBuffer = new FrameBuffer(SpectrumPalette.White);
        var borderRenderer = new Border(screenBuffer);
        borderRenderer.Update(SpectrumPalette.Red);

        var ticks = 0;
        while (ticks < 69888)
        {
            ticks += random.Next(4, 1000);
            borderRenderer.Update(SpectrumPalette.Red, ticks);
        }

        BorderShouldHaveColor(SpectrumPalette.Red, screenBuffer.Pixels);
    }

    [Fact]
    public void BorderRenderer_ShouldMatchAquaplane()
    {
        var screenBuffer = new FrameBuffer(SpectrumPalette.White);
        var borderRenderer = new Border(screenBuffer);

        borderRenderer.Update(SpectrumPalette.Cyan, 1);
        borderRenderer.Update(SpectrumPalette.Cyan, 145);
        borderRenderer.Update(SpectrumPalette.Blue, 25013);
        borderRenderer.Update(SpectrumPalette.Blue, 69888);

        screenBuffer.Pixels[..52].Should().AllSatisfy(c => c.Should().Be(SpectrumPalette.White));
        screenBuffer.Pixels[52..351].Should().AllSatisfy(c => c.Should().Be(SpectrumPalette.Cyan));
        //screenBuffer[48..351].Should().AllSatisfy(c => c.Should().Be(Colors.Cyan));
    }

    private static void BorderShouldHaveColor(Color color, Color[] screenBuffer)
    {
        screenBuffer[0..47].Should().AllSatisfy(c => c.Should().Be(SpectrumPalette.White));
        screenBuffer[48..22576].Should().AllSatisfy(c => c.Should().Be(color));
        for (var i = 0; i < 192; i++)
        {
            screenBuffer.Skip(22528 + (i * 352)).Take(48).Should().AllSatisfy(c => c.Should().Be(color));
            screenBuffer.Skip(22576 + (i * 352)).Take(256).Should().AllSatisfy(c => c.Should().Be(SpectrumPalette.White));
            screenBuffer.Skip(22832 + (i * 352)).Take(48).Should().AllSatisfy(c => c.Should().Be(color));
        }
        screenBuffer[90112..].Should().AllSatisfy(c => c.Should().Be(color));
    }
}