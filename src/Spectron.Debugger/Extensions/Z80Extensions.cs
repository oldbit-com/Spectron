namespace OldBit.Spectron.Debugger.Extensions;

public static class Z80Extensions
{
    public static int GetRegisterValue(this Z80Cpu.Z80 cpu, string register) => register.ToUpper() switch
    {
        "A" => cpu.Registers.A,
        "B" => cpu.Registers.B,
        "C" => cpu.Registers.C,
        "D" => cpu.Registers.D,
        "E" => cpu.Registers.E,
        "H" => cpu.Registers.H,
        "L" => cpu.Registers.L,
        "I" => cpu.Registers.I,
        "R" => cpu.Registers.R,
        "IXH" => cpu.Registers.IXH,
        "IXL" => cpu.Registers.IXL,
        "IYH" => cpu.Registers.IYH,
        "IYL" => cpu.Registers.IYL,
        "AF" => cpu.Registers.AF,
        "AF'" => cpu.Registers.Prime.AF,
        "BC" => cpu.Registers.BC,
        "BC'" => cpu.Registers.Prime.BC,
        "DE" => cpu.Registers.DE,
        "DE'" => cpu.Registers.Prime.DE,
        "HL" => cpu.Registers.HL,
        "HL'" => cpu.Registers.Prime.HL,
        "IX" => cpu.Registers.IX,
        "IY" => cpu.Registers.IY,
        "SP" => cpu.Registers.SP,
        "PC" => cpu.Registers.PC,
        _ => throw new ArgumentException($"Unknown register {register}")
    };
}