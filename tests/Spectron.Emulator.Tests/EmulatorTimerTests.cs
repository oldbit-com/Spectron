using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Emulator.Tests;

public class EmulatorTimerTests
{
    [Fact]
    public async Task Pause_BeforeStart_ShouldKeepTimerPaused()
    {
        var timer = new EmulatorTimer { Interval = TimeSpan.FromMilliseconds(5) };
        var elapsedCount = 0;
        timer.Elapsed += (_, _) => Interlocked.Increment(ref elapsedCount);

        try
        {
            timer.Pause();
            timer.Start();

            await Task.Delay(150, TestContext.Current.CancellationToken);

            timer.IsPaused.ShouldBeTrue();
            elapsedCount.ShouldBe(0);
        }
        finally
        {
            timer.Stop();
            timer.Dispose();
        }
    }

    [Fact]
    public async Task Resume_AfterPauseBeforeStart_ShouldRunTimer()
    {
        var timer = new EmulatorTimer { Interval = TimeSpan.FromMilliseconds(5) };
        var elapsedCount = 0;
        timer.Elapsed += (_, _) => Interlocked.Increment(ref elapsedCount);

        try
        {
            timer.Pause();
            timer.Start();
            timer.Resume();

            await Task.Delay(300, TestContext.Current.CancellationToken);

            elapsedCount.ShouldBeGreaterThan(0);
        }
        finally
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
