using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Input;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Tests.Input;

public class KeyboardHandlerTests
{
    [Theory]
    [MemberData(nameof(KeyTestData.StandardKeyData), MemberType = typeof(KeyTestData))]
    public void StandardKeyPressed_IsMappedToSpectrumKey(Key key, SpectrumKey spectrumKey)
    {
        var handler = new KeyboardHandler();

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyPressed += h,
            h => handler.SpectrumKeyPressed -= h,
            () => handler.KeyPressed(new KeyEventArgs { Key = key }));

        raised.Arguments.IsKeyPressed.ShouldBeTrue();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Key.ShouldBe(key);
        raised.Arguments.Keys.ShouldBeEquivalentTo(new List<SpectrumKey> { spectrumKey });
    }

    [Theory]
    [MemberData(nameof(KeyTestData.StandardKeyData), MemberType = typeof(KeyTestData))]
    public void StandardKeyReleased_IsMappedToSpectrumKey(Key key, SpectrumKey spectrumKey)
    {
        var handler = new KeyboardHandler();

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyReleased += h,
            h => handler.SpectrumKeyReleased -= h,
            () => handler.KeyReleased(new KeyEventArgs { Key = key }));

        raised.Arguments.IsKeyPressed.ShouldBeFalse();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Key.ShouldBe(key);
        raised.Arguments.Keys.ShouldBeEquivalentTo(new List<SpectrumKey> { spectrumKey });
    }

    [Fact]
    public void CapsShiftPressed_IsMappedToSpectrumKey()
    {
        var handler = new KeyboardHandler();
        var settings = new KeyboardSettings { CapsShiftKey = Key.LeftShift };
        handler.UpdateSettings(settings);

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyPressed += h,
            h => handler.SpectrumKeyPressed -= h,
            () => handler.KeyPressed(new KeyEventArgs { Key = Key.LeftShift }));

        raised.Arguments.IsKeyPressed.ShouldBeTrue();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Key.ShouldBe(Key.LeftShift);
        raised.Arguments.Keys.ShouldBeEquivalentTo(new List<SpectrumKey> { SpectrumKey.CapsShift });
    }

    [Fact]
    public void SymbolShiftPressed_IsMappedToSpectrumKey()
    {
        var handler = new KeyboardHandler();
        var settings = new KeyboardSettings { SymbolShiftKey = Key.RightAlt };
        handler.UpdateSettings(settings);

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyPressed += h,
            h => handler.SpectrumKeyPressed -= h,
            () => handler.KeyPressed(new KeyEventArgs { Key = Key.RightAlt }));

        raised.Arguments.IsKeyPressed.ShouldBeTrue();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Key.ShouldBe(Key.RightAlt);
        raised.Arguments.Keys.ShouldBeEquivalentTo(new List<SpectrumKey> { SpectrumKey.SymbolShift });
    }

    [Theory]
    [MemberData(nameof(KeyTestData.ExtraKeyData), MemberType = typeof(KeyTestData))]
    public void ExtraKeyPressed_IsMappedToSpectrumKey(Key key, List<SpectrumKey> spectrumKeys)
    {
        var handler = new KeyboardHandler();
        handler.UpdateSettings(new KeyboardSettings { ShouldHandleExtendedKeys = true });

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyPressed += h,
            h => handler.SpectrumKeyPressed -= h,
            () => handler.KeyPressed(new KeyEventArgs { Key = key }));

        raised.Arguments.IsKeyPressed.ShouldBeTrue();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Key.ShouldBe(key);
        raised.Arguments.Keys.ShouldBeEquivalentTo(spectrumKeys);
    }

    [Theory]
    [MemberData(nameof(KeyTestData.MoreExtraKeyData), MemberType = typeof(KeyTestData))]
    public void MoreExtraKeyPressed_IsMappedToSpectrumKey(string key, List<SpectrumKey> spectrumKeys)
    {
        var handler = new KeyboardHandler();
        handler.UpdateSettings(new KeyboardSettings { ShouldHandleExtendedKeys = true });

        var raised = Assert.Raises<SpectrumKeyEventArgs>(
            h => handler.SpectrumKeyPressed += h,
            h => handler.SpectrumKeyPressed -= h,
            () => handler.KeyPressed(new KeyEventArgs { KeySymbol = key }));

        raised.Arguments.IsKeyPressed.ShouldBeTrue();
        raised.Arguments.IsSimulated.ShouldBeFalse();
        raised.Arguments.Keys.ShouldBeEquivalentTo(spectrumKeys);
    }

    [Theory]
    [InlineData("[", SpectrumKey.Y)]
    [InlineData("]", SpectrumKey.U)]
    [InlineData("{", SpectrumKey.F)]
    [InlineData("}", SpectrumKey.G)]
    [InlineData("`", SpectrumKey.A)]
    [InlineData("\\", SpectrumKey.D)]
    [InlineData("|", SpectrumKey.S)]
    public async Task ExtendedModeKey_IsMappedToSpectrumKeySequence(string key, SpectrumKey spectrumKey)
    {
        var handler = new KeyboardHandler();
        handler.UpdateSettings(new KeyboardSettings { ShouldHandleExtendedKeys = true });
        var keyEvents = new List<SpectrumKeyEventArgs>();

        handler.SpectrumKeyReleased += (_, args) => keyEvents.Add(args);
        handler.SpectrumKeyPressed += (_, args) => keyEvents.Add(args);

        handler.KeyPressed(new KeyEventArgs { KeySymbol = key });

        // Extended mode keys are using a sequence of key presses and releases with a delay
        // We need to wait for the sequence to complete
        await Task.Delay(100, TestContext.Current.CancellationToken);

        keyEvents.Count.ShouldBe(5);

        keyEvents[0].IsKeyPressed.ShouldBeTrue();
        keyEvents[0].Keys[0].ShouldBe(SpectrumKey.CapsShift);

        keyEvents[1].IsKeyPressed.ShouldBeTrue();
        keyEvents[1].Keys[0].ShouldBe(SpectrumKey.SymbolShift);

        keyEvents[2].IsKeyPressed.ShouldBeFalse();
        keyEvents[2].Keys[0].ShouldBe(SpectrumKey.CapsShift);

        keyEvents[3].IsKeyPressed.ShouldBeTrue();
        keyEvents[3].Keys[0].ShouldBe(spectrumKey);

        keyEvents[4].IsKeyPressed.ShouldBeFalse();
        keyEvents[4].Keys[0].ShouldBe(SpectrumKey.SymbolShift);
    }
}