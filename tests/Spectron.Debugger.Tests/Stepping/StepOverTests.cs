using OldBit.Spectron.Debugger.Stepping;
using OldBit.Spectron.Debugger.Tests.Fixtures;

namespace OldBit.Spectron.Debugger.Tests.Stepping;

public class StepOverTests
{
    private const Word Address = 0x8000;

    [Theory]
    [InlineData(new byte[] { 0xCD, 0x00, 0x90 }, 3)]        // CALL nn
    [InlineData(new byte[] { 0xC4, 0x00, 0x90 }, 3)]        // CALL NZ,nn
    [InlineData(new byte[] { 0xFC, 0x00, 0x90 }, 3)]        // CALL M,nn
    [InlineData(new byte[] { 0xC2, 0x00, 0x90 }, 3)]        // JP NZ,nn
    [InlineData(new byte[] { 0xC7 }, 1)]                    // RST 0
    [InlineData(new byte[] { 0xFF }, 1)]                    // RST 38h
    [InlineData(new byte[] { 0x10, 0xFE }, 2)]              // DJNZ
    [InlineData(new byte[] { 0x20, 0xFE }, 2)]              // JR NZ
    [InlineData(new byte[] { 0x38, 0xFE }, 2)]              // JR C
    [InlineData(new byte[] { 0x76 }, 1)]                    // HALT
    [InlineData(new byte[] { 0xED, 0xB0 }, 2)]              // LDIR
    [InlineData(new byte[] { 0xED, 0xB1 }, 2)]              // CPIR
    [InlineData(new byte[] { 0xED, 0xBB }, 2)]              // OTDR
    public void GetReturnAddress_WhenInstructionCanBeSteppedOver_ShouldReturnNextAddress(byte[] code, int length)
    {
        var emulator = TestEmulator.Create();
        emulator.WriteMemory(Address, code);

        StepOver.GetReturnAddress(emulator.Memory, Address).ShouldBe((Word)(Address + length));
    }

    [Theory]
    [InlineData(new byte[] { 0x00 })]                       // NOP
    [InlineData(new byte[] { 0xC3, 0x00, 0x90 })]           // JP nn
    [InlineData(new byte[] { 0x18, 0xFE })]                 // JR e
    [InlineData(new byte[] { 0xC9 })]                       // RET
    [InlineData(new byte[] { 0xED, 0xA0 })]                 // LDI
    [InlineData(new byte[] { 0xDD, 0xE9 })]                 // JP (IX)
    public void GetReturnAddress_WhenSingleStepIsEnough_ShouldReturnNull(byte[] code)
    {
        var emulator = TestEmulator.Create();
        emulator.WriteMemory(Address, code);

        StepOver.GetReturnAddress(emulator.Memory, Address).ShouldBeNull();
    }
}
