using OldBit.Z80Cpu.Registers;

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
        "F" => (int)cpu.Registers.F,
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
        "PC" => cpu.Registers.PC,
        "SP" => cpu.Registers.SP,
        _ => throw new ArgumentException($"Unknown register {register}")
    };

    public static void SetRegisterValue(this Z80Cpu.Z80 cpu, string register, int value)
    {
        switch (register.ToUpper())
        {
            case "A":
                cpu.Registers.A = (byte)value;
                break;

            case "B":
                cpu.Registers.B = (byte)value;
                break;

            case "C":
                cpu.Registers.C = (byte)value;
                break;

            case "D":
                cpu.Registers.D = (byte)value;
                break;

            case "E":
                cpu.Registers.E = (byte)value;
                break;

            case "F":
                cpu.Registers.F = (Flags)value;
                break;

            case "H":
                cpu.Registers.H = (byte)value;
                break;

            case "L":
                cpu.Registers.L = (byte)value;
                break;

            case "I":
                cpu.Registers.I = (byte)value;
                break;

            case "R":
                cpu.Registers.R = (byte)value;
                break;

            case "IXH":
                cpu.Registers.IXH = (byte)value;
                break;

            case "IXL":
                cpu.Registers.IXL = (byte)value;
                break;

            case "IYH":
                cpu.Registers.IYH = (byte)value;
                break;

            case "IYL":
                cpu.Registers.IYL = (byte)value;
                break;

            case "AF":
                cpu.Registers.AF = (Word)value;
                break;

            case "AF'":
                cpu.Registers.Prime.AF = (Word)value;
                break;

            case "BC":
                cpu.Registers.BC = (Word)value;
                break;

            case "BC'":
                cpu.Registers.Prime.BC = (Word)value;
                break;

            case "DE":
                cpu.Registers.DE = (Word)value;
                break;

            case "DE'":
                cpu.Registers.Prime.DE = (Word)value;
                break;

            case "HL":
                cpu.Registers.HL = (Word)value;
                break;

            case "HL'":
                cpu.Registers.Prime.HL = (Word)value;
                break;

            case "IX":
                cpu.Registers.IX = (Word)value;
                break;

            case "IY":
                cpu.Registers.IY = (Word)value;
                break;

            case "PC":
                cpu.Registers.PC = (Word)value;
                break;

            case "SP":
                cpu.Registers.SP = (Word)value;
                break;

            default:
                throw new ArgumentException($"Unknown register {register}");
        }
    }
}