using System.Reflection;
using System.Text;

namespace OldBit.Spectron.Disassembly.Tests;

public class DisassemblyTests
{
    [Fact]
    public void Rom_ShouldDisassembleToCode1()
    {
        var rom = LoadRomFile();

        var disassembler = new Disassembler(rom, 0x11B7, 66);
        var instructions = disassembler.Disassemble();

        var code = new StringBuilder();

        foreach (var instruction in instructions)
        {
            code.Append($"{instruction.Address:X4}   {instruction.Code}\n");
        }

        code.ToString().ShouldBe(
            "11B7   DI\n" +
            "11B8   LD A,$FF\n" +
            "11BA   LD DE,($5CB2)\n" +
            "11BE   EXX\n" +
            "11BF   LD BC,($5CB4)\n" +
            "11C3   LD DE,($5C38)\n" +
            "11C7   LD HL,($5C7B)\n" +
            "11CA   EXX\n" +
            "11CB   LD B,A\n" +
            "11CC   LD A,$07\n" +
            "11CE   OUT ($FE),A\n" +
            "11D0   LD A,$3F\n" +
            "11D2   LD I,A\n" +
            "11D4   NOP\n" +
            "11D5   NOP\n" +
            "11D6   NOP\n" +
            "11D7   NOP\n" +
            "11D8   NOP\n" +
            "11D9   NOP\n" +
            "11DA   LD H,D\n" +
            "11DB   LD L,E\n" +
            "11DC   LD (HL),$02\n" +
            "11DE   DEC HL\n" +
            "11DF   CP H\n" +
            "11E0   JR NZ,$11DC\n" +
            "11E2   AND A\n" +
            "11E3   SBC HL,DE\n" +
            "11E5   ADD HL,DE\n" +
            "11E6   INC HL\n" +
            "11E7   JR NC,$11EF\n" +
            "11E9   DEC (HL)\n" +
            "11EA   JR Z,$11EF\n" +
            "11EC   DEC (HL)\n" +
            "11ED   JR Z,$11E2\n" +
            "11EF   DEC HL\n" +
            "11F0   EXX\n" +
            "11F1   LD ($5CB4),BC\n" +
            "11F5   LD ($5C38),DE\n" +
            "11F9   LD ($5C7B),HL\n" +
            "11FC   EXX\n" +
            "11FD   INC B\n" +
            "11FE   JR Z,$1219\n" +
            "1200   LD ($5CB4),HL\n" +
            "1203   LD DE,$3EAF\n" +
            "1206   LD BC,$00A8\n" +
            "1209   EX DE,HL\n" +
            "120A   LDDR\n" +
            "120C   EX DE,HL\n" +
            "120D   INC HL\n" +
            "120E   LD ($5C7B),HL\n" +
            "1211   DEC HL\n"+
            "1212   LD BC,$0040\n" +
            "1215   LD ($5C38),BC\n" +
            "1219   LD ($5CB2),HL\n" +
            "121C   LD HL,$3C00\n" +
            "121F   LD ($5C36),HL\n" +
            "1222   LD HL,($5CB2)\n" +
            "1225   LD (HL),$3E\n" +
            "1227   DEC HL\n" +
            "1228   LD SP,HL\n" +
            "1229   DEC HL\n" +
            "122A   DEC HL\n" +
            "122B   LD ($5C3D),HL\n" +
            "122E   IM 1\n" +
            "1230   LD IY,$5C3A\n" +
            "1234   EI\n"
        );
    }

    [Fact]
    public void Rom_ShouldDisassembleToCode2()
    {
        var rom = LoadRomFile();

        var disassembler = new Disassembler(rom, 0x0C55, 13);
        var instructions = disassembler.Disassemble();

        var code = new StringBuilder();

        foreach (var instruction in instructions)
        {
            code.Append($"{instruction.Address:X4}   {instruction.Code}\n");
        }

        code.ToString().ShouldBe(
            "0C55   BIT 1,(IY+$01)\n" +
            "0C59   RET NZ\n" +
            "0C5A   LD DE,$0DD9\n" +
            "0C5D   PUSH DE\n" +
            "0C5E   LD A,B\n" +
            "0C5F   BIT 0,(IY+$02)\n" +
            "0C63   JP NZ,$0D02\n" +
            "0C66   CP (IY+$31)\n" +
            "0C69   JR C,$0C86\n" +
            "0C6B   RET NZ\n" +
            "0C6C   BIT 4,(IY+$02)\n" +
            "0C70   JR Z,$0C88\n" +
            "0C72   LD E,(IY+$2D)\n"
        );
    }

    [Fact]
    public void MemoryBoundary_ShouldDisassembly()
    {
        var rom = LoadRomFile();
        var memory = new byte[65536];
        Array.Copy(rom, memory, rom.Length);
        memory[65534] = 0xC3; // JP 0x0000
        memory[65535] = 0x21;

        var disassembler = new Disassembler(memory, 65534, 4);
        var instructions = disassembler.Disassemble();

        var code = new StringBuilder();

        foreach (var instruction in instructions)
        {
            code.Append($"{instruction.Address:X4}   {instruction.Code}\n");
        }

        code.ToString().ShouldBe(
            "FFFE   JP $F321\n" +
            "0001   XOR A\n" +
            "0002   LD DE,$FFFF\n" +
            "0005   JP $11CB\n"
        );
    }

    private static byte[] LoadRomFile()
    {
        var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        return File.ReadAllBytes(Path.Combine(binFolder, "TestFiles", "48.rom"));
    }
}