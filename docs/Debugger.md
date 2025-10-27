### Shortcuts
- **Step Over** - `F10` \
  For `CALL`, `JR cc`, `JP cc`, `DJNZ`, `LDIR` or `LDDR` instructions, debugger will try to step over the subroutine.
- **Step Into** - `F11` \
  Debugger will step into the subroutine call or conditional jump.
- **Step Out** - `Shift + F11` \
  Debugger will step out of the subroutine using the current return address on the stack.
  So this will only work if the value on the stack contains return address.

### Breakpoints

Two types of breakpoints are supported: on register value change or on memory write

#### Register breakpoints
Typical scenario is use a condition for PC register, for example `PC==32768`. This would break execution
at the specified address. However any register can be used, for example `HL==16384`, `A==0x79`etc.

#### Memory breakpoints
Memory breakpoints can have two forms: trigger when specific value is written to e memory cell or more generic
when memory cell is written. For example `16384==32` will break when value `32` has been written to `16384` memory address.
If condition is just `16384`, execution will break when any value  has been written to that address.

### Immediate window instructions, case insensitive:
- `HELP` - print help information
- `PRINT [expression]` or `? [expression]` - prints a value of the expression
- `GOTO address` - sets the program counter to the address, equivalent of `PC=address`
- `LIST [address]` - disassembles the code, if no address is provided, it will disassemble the current PC
- `POKE address,value` - writes a value to the memory
- `PEEK address` - reads a value from the memory
- `OUT `port,value` - writes a value to the IO port
- `IN port` - reads a value from the IO port
- `SAVE "filename" [address [,length]]` - saves a memory to a file in binary format starting from the optional address optional length
- `R=value` - sets a register value

#### Register arguments:
`A B C D E H L F IXH IXL IYH IYL AF BC DE HL IX IY SP PC I R AF' BC' DE' HL'`

#### Number arguments:
- Decimal: `16384`
- Hexadecimal: `0x4000` or `$4000` or `4000h`
- Binary: `0b1010` or `1010b`

#### Examples:
- `PRINT HL`
- `GOTO PC`, `GOTO 32768`
- `LIST`, `LIST 32768`
- `POKE 16384,0x3E`
- `PEEK 16384`
- `OUT 0xFE,$3E`
- `IN 254`
- `SAVE "screen.bin" 16384,6912`
- `A=10h`, `IXH=A` `HL'=DE`, `IX=0x4000`, `SP=0x8000`, etc.

> [!NOTE]  
> All instructions and operands are case-insensitive.
> 