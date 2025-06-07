using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Tap;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Emulator.Tests.Tape;

public class PulseFactoryTests
{
    [Fact]
    public void Pulses_ShouldBeCreatedForStandardSpeedDataBlock()
    {
        var block = CreateStandardSpeedDataBlock();
        var pulses = PulseFactory.Create(block, Hardware.Spectrum48K);

        pulses.PilotHeaderPulse.ShouldNotBeNull();
        pulses.PilotHeaderPulse.IsSilence.ShouldBeFalse();
        pulses.PilotHeaderPulse.RepeatCount.ShouldBe(8063);
        pulses.PilotHeaderPulse.Duration.ShouldBe(2168);

        pulses.PilotDataPulse.ShouldNotBeNull();
        pulses.PilotDataPulse.IsSilence.ShouldBeFalse();
        pulses.PilotDataPulse.RepeatCount.ShouldBe(3223);
        pulses.PilotDataPulse.Duration.ShouldBe(2168);

        pulses.FirstSyncPulse.ShouldNotBeNull();
        pulses.FirstSyncPulse.IsSilence.ShouldBeFalse();
        pulses.FirstSyncPulse.RepeatCount.ShouldBe(1);
        pulses.FirstSyncPulse.Duration.ShouldBe(667);

        pulses.SecondSyncPulse.ShouldNotBeNull();
        pulses.SecondSyncPulse.IsSilence.ShouldBeFalse();
        pulses.SecondSyncPulse.RepeatCount.ShouldBe(1);
        pulses.SecondSyncPulse.Duration.ShouldBe(735);

        pulses.PausePulse.ShouldNotBeNull();
        pulses.PausePulse.IsSilence.ShouldBeTrue();
        pulses.PausePulse.RepeatCount.ShouldBe(1);
        pulses.PausePulse.Duration.ShouldBe(349440);

        pulses.Data.TotalCount.ShouldBe(56);
        pulses.Data.Pulses.ShouldAllBe(x => x.IsSilence == false);
        pulses.Data.Pulses.ShouldAllBe(x => x.RepeatCount == 2);
        pulses.Data.Pulses.Select(x => x.Duration).ShouldBe(
        [
            855, 855, 855, 855, 855, 855, 855, 855, 855, 855, 855, 855, 855, 855, 855,
            1710, 855, 855, 855, 855, 855, 855, 1710, 855, 855, 855, 855, 855, 855, 855,
            1710, 1710, 855, 855, 855, 855, 855, 1710, 855, 855, 855, 855, 855, 855, 855,
            1710, 855, 1710, 855, 855, 855, 855, 855, 855, 855, 1710
        ]);
    }

    [Fact]
    public void Pulses_ShouldBeCreatedForTurboSpeedDataBlockHeader()
    {
        var block = CreateTurboSpeedDataBlock(isHeader: true);
        var pulses = PulseFactory.Create(block, Hardware.Spectrum48K);

        pulses.PilotHeaderPulse.ShouldNotBeNull();
        pulses.PilotHeaderPulse.IsSilence.ShouldBeFalse();
        pulses.PilotHeaderPulse.RepeatCount.ShouldBe(3000);
        pulses.PilotHeaderPulse.Duration.ShouldBe(4000);

        pulses.PilotDataPulse.ShouldNotBeNull();
        pulses.PilotDataPulse.IsSilence.ShouldBeFalse();
        pulses.PilotDataPulse.RepeatCount.ShouldBe(3223);
        pulses.PilotDataPulse.Duration.ShouldBe(4000);

        pulses.FirstSyncPulse.ShouldNotBeNull();
        pulses.FirstSyncPulse.IsSilence.ShouldBeFalse();
        pulses.FirstSyncPulse.RepeatCount.ShouldBe(1);
        pulses.FirstSyncPulse.Duration.ShouldBe(600);

        pulses.SecondSyncPulse.ShouldNotBeNull();
        pulses.SecondSyncPulse.IsSilence.ShouldBeFalse();
        pulses.SecondSyncPulse.RepeatCount.ShouldBe(1);
        pulses.SecondSyncPulse.Duration.ShouldBe(700);

        pulses.PausePulse.ShouldBeNull();

        pulses.Data.TotalCount.ShouldBe(49);
        pulses.Data.Pulses.ShouldAllBe(x => x.IsSilence == false);
        pulses.Data.Pulses.ShouldAllBe(x => x.RepeatCount == 2);
        pulses.Data.Pulses.Select(x => x.Duration).ShouldBe(
        [
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4,
            4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 5, 5, 4, 4, 4, 4, 4, 5, 4, 4,
            4, 4, 4, 4, 4, 5, 4, 5, 4
        ]);
    }

    [Fact]
    public void Pulses_ShouldBeCreatedForTurboSpeedDataBlockData()
    {
        var block = CreateTurboSpeedDataBlock(isHeader: false);
        var pulses = PulseFactory.Create(block, Hardware.Spectrum48K);

        pulses.PilotHeaderPulse.ShouldNotBeNull();
        pulses.PilotHeaderPulse.IsSilence.ShouldBeFalse();
        pulses.PilotHeaderPulse.RepeatCount.ShouldBe(8063);
        pulses.PilotHeaderPulse.Duration.ShouldBe(4000);

        pulses.PilotDataPulse.ShouldNotBeNull();
        pulses.PilotDataPulse.IsSilence.ShouldBeFalse();
        pulses.PilotDataPulse.RepeatCount.ShouldBe(3000);
        pulses.PilotDataPulse.Duration.ShouldBe(4000);

        pulses.FirstSyncPulse.ShouldNotBeNull();
        pulses.FirstSyncPulse.IsSilence.ShouldBeFalse();
        pulses.FirstSyncPulse.RepeatCount.ShouldBe(1);
        pulses.FirstSyncPulse.Duration.ShouldBe(600);

        pulses.SecondSyncPulse.ShouldNotBeNull();
        pulses.SecondSyncPulse.IsSilence.ShouldBeFalse();
        pulses.SecondSyncPulse.RepeatCount.ShouldBe(1);
        pulses.SecondSyncPulse.Duration.ShouldBe(700);

        pulses.PausePulse.ShouldBeNull();

        pulses.Data.TotalCount.ShouldBe(49);
        pulses.Data.Pulses.ShouldAllBe(x => x.IsSilence == false);
        pulses.Data.Pulses.ShouldAllBe(x => x.RepeatCount == 2);
        pulses.Data.Pulses.Select(x => x.Duration).ShouldBe(
        [

            5, 5, 5, 5, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 5, 4,
            4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 5, 5, 4, 4,
            4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 5, 4, 5, 4
        ]);
    }

    [Fact]
    public void PausePulse_ShouldBeNull_IfPauseIsZeroMilliseconds()
    {
        var pulse = PulseFactory.CreatePausePulse(0, Hardware.Spectrum48K);

        pulse.ShouldBeNull();
    }

    [Fact]
    public void Pulses_ShouldBeCreatedForPureDataBlock()
    {
        var block = CreatePureDataBlock();
        var pulses = PulseFactory.Create(block, Hardware.Spectrum48K);

        pulses.PilotHeaderPulse.ShouldBeNull();
        pulses.PilotDataPulse.ShouldBeNull();
        pulses.FirstSyncPulse.ShouldBeNull();
        pulses.SecondSyncPulse.ShouldBeNull();

        pulses.PausePulse.ShouldNotBeNull();
        pulses.PausePulse.IsSilence.ShouldBeTrue();
        pulses.PausePulse.RepeatCount.ShouldBe(1);
        pulses.PausePulse.Duration.ShouldBe(34944);

        pulses.Data.TotalCount.ShouldBe(35);
        pulses.Data.Pulses.ShouldAllBe(x => x.IsSilence == false);
        pulses.Data.Pulses.ShouldAllBe(x => x.RepeatCount == 2);
        pulses.Data.Pulses.Select(x => x.Duration).ShouldBe(
        [
            4, 4, 4, 4, 4, 4, 4, 3, 4, 4, 4, 4, 4, 4, 3, 4, 4,
            4, 4, 4, 4, 4, 3, 3, 4, 4, 4, 4, 4, 3, 4, 4, 4, 4, 4
        ]);
    }

    [Fact]
    public void Pulses_ShouldBeCreatedForPureToneBlock()
    {
        var block = CreatePureToneBlock();
        var pulse = PulseFactory.Create(block);

        pulse.ShouldNotBeNull();
        pulse.IsSilence.ShouldBeFalse();
        pulse.RepeatCount.ShouldBe(3);
        pulse.Duration.ShouldBe(10);
    }

    [Fact]
    public void Pulses_ShouldBeCreatedForPulseSequenceBlock()
    {
        var block = CreatePulseSequenceBlock();
        var pulses = PulseFactory.Create(block).ToList();

        pulses.Count.ShouldBe(3);
        pulses.ShouldAllBe(x => x.IsSilence == false);
        pulses.ShouldAllBe(x => x.RepeatCount == 1);

        pulses[0].Duration.ShouldBe(1);
        pulses[1].Duration.ShouldBe(3);
        pulses[2].Duration.ShouldBe(7);
    }

    [Fact]
    public void PausePulse_ShouldBeCreated()
    {
        var pulse = PulseFactory.CreatePausePulse(1000, Hardware.Spectrum48K);

        pulse.ShouldNotBeNull();
        pulse.IsSilence.ShouldBeTrue();
        pulse.RepeatCount.ShouldBe(1);
        pulse.Duration.ShouldBe(3494400);
    }

    private static StandardSpeedDataBlock CreateStandardSpeedDataBlock()
    {
        var tapData = new TapData(0, [1, 2, 3, 4, 5]);
        var block = new StandardSpeedDataBlock(tapData)
        {
            PauseDuration = 100
        };

        return block;
    }

    private static TurboSpeedDataBlock CreateTurboSpeedDataBlock(bool isHeader)
    {
        var block = new TurboSpeedDataBlock
        {
            PilotPulseLength = 4000,
            PilotToneLength = 3000,
            FirstSyncPulseLength = 600,
            SecondSyncPulseLength = 700,
            ZeroBitPulseLength = 4,
            OneBitPulseLength = 5,
            UsedBitsInLastByte = 1,
        };

        var flag = (byte)(isHeader ? 0 : 0xFF);
        block.Data.AddRange([flag, 1,2,3,4,5,6]);

        return block;
    }

    private static PureDataBlock CreatePureDataBlock()
    {
        var block = new PureDataBlock
        {
            OneBitPulseLength = 3,
            ZeroBitPulseLength = 4,
            PauseDuration = 10,
            UsedBitsInLastByte = 3,
        };

        block.Data.AddRange([1,2,3,4,5,]);

        return block;
    }

    private static PureToneBlock CreatePureToneBlock() => new()
    {
        PulseCount = 3,
        PulseLength = 10,
    };

    private static PulseSequenceBlock CreatePulseSequenceBlock()
    {
        var block = new PulseSequenceBlock();
        block.PulseLengths.AddRange([1, 3, 7]);

        return block;
    }
}