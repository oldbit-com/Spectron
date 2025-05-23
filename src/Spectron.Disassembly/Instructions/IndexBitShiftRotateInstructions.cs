namespace OldBit.Spectron.Disassembly.Instructions;

internal static class IndexBitShiftRotateInstructions
{
    internal static Dictionary<int, InstructionTemplate> Index { get; } = new()
    {
        { 0x00, new InstructionTemplate("RLC (<xy><d>),B", IsUndocumented: true) },
        { 0x01, new InstructionTemplate("RLC (<xy><d>),C", IsUndocumented: true) },
        { 0x02, new InstructionTemplate("RLC (<xy><d>),D", IsUndocumented: true) },
        { 0x03, new InstructionTemplate("RLC (<xy><d>),E", IsUndocumented: true) },
        { 0x04, new InstructionTemplate("RLC (<xy><d>),H", IsUndocumented: true) },
        { 0x05, new InstructionTemplate("RLC (<xy><d>),L", IsUndocumented: true) },
        { 0x06, new InstructionTemplate("RLC (<xy><d>)") },
        { 0x07, new InstructionTemplate("RLC (<xy><d>),A", IsUndocumented: true) },
        { 0x08, new InstructionTemplate("RRC (<xy><d>),B", IsUndocumented: true) },
        { 0x09, new InstructionTemplate("RRC (<xy><d>),C", IsUndocumented: true) },
        { 0x0A, new InstructionTemplate("RRC (<xy><d>),D", IsUndocumented: true) },
        { 0x0B, new InstructionTemplate("RRC (<xy><d>),E", IsUndocumented: true) },
        { 0x0C, new InstructionTemplate("RRC (<xy><d>),H", IsUndocumented: true) },
        { 0x0D, new InstructionTemplate("RRC (<xy><d>),L", IsUndocumented: true) },
        { 0x0E, new InstructionTemplate("RRC (<xy><d>)") },
        { 0x0F, new InstructionTemplate("RRC (<xy><d>),A", IsUndocumented: true) },
        { 0x10, new InstructionTemplate("RL (<xy><d>),B", IsUndocumented: true) },
        { 0x11, new InstructionTemplate("RL (<xy><d>),C", IsUndocumented: true) },
        { 0x12, new InstructionTemplate("RL (<xy><d>),D", IsUndocumented: true) },
        { 0x13, new InstructionTemplate("RL (<xy><d>),E", IsUndocumented: true) },
        { 0x14, new InstructionTemplate("RL (<xy><d>),H", IsUndocumented: true) },
        { 0x15, new InstructionTemplate("RL (<xy><d>),L", IsUndocumented: true) },
        { 0x16, new InstructionTemplate("RL (<xy><d>)") },
        { 0x17, new InstructionTemplate("RL (<xy><d>),A", IsUndocumented: true) },
        { 0x18, new InstructionTemplate("RR (<xy><d>),B", IsUndocumented: true) },
        { 0x19, new InstructionTemplate("RR (<xy><d>),C", IsUndocumented: true) },
        { 0x1A, new InstructionTemplate("RR (<xy><d>),D", IsUndocumented: true) },
        { 0x1B, new InstructionTemplate("RR (<xy><d>),E", IsUndocumented: true) },
        { 0x1C, new InstructionTemplate("RR (<xy><d>),H", IsUndocumented: true) },
        { 0x1D, new InstructionTemplate("RR (<xy><d>),L", IsUndocumented: true) },
        { 0x1E, new InstructionTemplate("RR (<xy><d>)") },
        { 0x1F, new InstructionTemplate("RR (<xy><d>),A", IsUndocumented: true) },
        { 0x20, new InstructionTemplate("SLA (<xy><d>),B", IsUndocumented: true) },
        { 0x21, new InstructionTemplate("SLA (<xy><d>),C", IsUndocumented: true) },
        { 0x22, new InstructionTemplate("SLA (<xy><d>),D", IsUndocumented: true) },
        { 0x23, new InstructionTemplate("SLA (<xy><d>),E", IsUndocumented: true) },
        { 0x24, new InstructionTemplate("SLA (<xy><d>),H", IsUndocumented: true) },
        { 0x25, new InstructionTemplate("SLA (<xy><d>),L", IsUndocumented: true) },
        { 0x26, new InstructionTemplate("SLA (<xy><d>)") },
        { 0x27, new InstructionTemplate("SLA (<xy><d>),A", IsUndocumented: true) },
        { 0x28, new InstructionTemplate("SRA (<xy><d>),B", IsUndocumented: true) },
        { 0x29, new InstructionTemplate("SRA (<xy><d>),C", IsUndocumented: true) },
        { 0x2A, new InstructionTemplate("SRA (<xy><d>),D", IsUndocumented: true) },
        { 0x2B, new InstructionTemplate("SRA (<xy><d>),E", IsUndocumented: true) },
        { 0x2C, new InstructionTemplate("SRA (<xy><d>),H", IsUndocumented: true) },
        { 0x2D, new InstructionTemplate("SRA (<xy><d>),L", IsUndocumented: true) },
        { 0x2E, new InstructionTemplate("SRA (<xy><d>)") },
        { 0x2F, new InstructionTemplate("SRA (<xy><d>),A", IsUndocumented: true) },
        { 0x30, new InstructionTemplate("SLL (<xy><d>),B", IsUndocumented: true) },
        { 0x31, new InstructionTemplate("SLL (<xy><d>),C", IsUndocumented: true) },
        { 0x32, new InstructionTemplate("SLL (<xy><d>),D", IsUndocumented: true) },
        { 0x33, new InstructionTemplate("SLL (<xy><d>),E", IsUndocumented: true) },
        { 0x34, new InstructionTemplate("SLL (<xy><d>),H", IsUndocumented: true) },
        { 0x35, new InstructionTemplate("SLL (<xy><d>),L", IsUndocumented: true) },
        { 0x36, new InstructionTemplate("SLL (<xy><d>)", IsUndocumented: true) },
        { 0x37, new InstructionTemplate("SLL (<xy><d>),A", IsUndocumented: true) },
        { 0x38, new InstructionTemplate("SRL (<xy><d>),B", IsUndocumented: true) },
        { 0x39, new InstructionTemplate("SRL (<xy><d>),C", IsUndocumented: true) },
        { 0x3A, new InstructionTemplate("SRL (<xy><d>),D", IsUndocumented: true) },
        { 0x3B, new InstructionTemplate("SRL (<xy><d>),E", IsUndocumented: true) },
        { 0x3C, new InstructionTemplate("SRL (<xy><d>),H", IsUndocumented: true) },
        { 0x3D, new InstructionTemplate("SRL (<xy><d>),L", IsUndocumented: true) },
        { 0x3E, new InstructionTemplate("SRL (<xy><d>)") },
        { 0x3F, new InstructionTemplate("SRL (<xy><d>),A", IsUndocumented: true) },
        { 0x40, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x41, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x42, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x43, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x44, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x45, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x46, new InstructionTemplate("BIT 0,(<xy><d>)") },
        { 0x47, new InstructionTemplate("BIT 0,(<xy><d>)", IsUndocumented: true) },
        { 0x48, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x49, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x4A, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x4B, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x4C, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x4D, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x4E, new InstructionTemplate("BIT 1,(<xy><d>)") },
        { 0x4F, new InstructionTemplate("BIT 1,(<xy><d>)", IsUndocumented: true) },
        { 0x50, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x51, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x52, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x53, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x54, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x55, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x56, new InstructionTemplate("BIT 2,(<xy><d>)") },
        { 0x57, new InstructionTemplate("BIT 2,(<xy><d>)", IsUndocumented: true) },
        { 0x58, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x59, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x5A, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x5B, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x5C, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x5D, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x5E, new InstructionTemplate("BIT 3,(<xy><d>)") },
        { 0x5F, new InstructionTemplate("BIT 3,(<xy><d>)", IsUndocumented: true) },
        { 0x60, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x61, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x62, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x63, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x64, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x65, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x66, new InstructionTemplate("BIT 4,(<xy><d>)") },
        { 0x67, new InstructionTemplate("BIT 4,(<xy><d>)", IsUndocumented: true) },
        { 0x68, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x69, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x6A, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x6B, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x6C, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x6D, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x6E, new InstructionTemplate("BIT 5,(<xy><d>)") },
        { 0x6F, new InstructionTemplate("BIT 5,(<xy><d>)", IsUndocumented: true) },
        { 0x70, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x71, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x72, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x73, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x74, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x75, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x76, new InstructionTemplate("BIT 6,(<xy><d>)") },
        { 0x77, new InstructionTemplate("BIT 6,(<xy><d>)", IsUndocumented: true) },
        { 0x78, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x79, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x7A, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x7B, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x7C, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x7D, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x7E, new InstructionTemplate("BIT 7,(<xy><d>)") },
        { 0x7F, new InstructionTemplate("BIT 7,(<xy><d>)", IsUndocumented: true) },
        { 0x80, new InstructionTemplate("RES 0,(<xy><d>),B", IsUndocumented: true) },
        { 0x81, new InstructionTemplate("RES 0,(<xy><d>),C", IsUndocumented: true) },
        { 0x82, new InstructionTemplate("RES 0,(<xy><d>),D", IsUndocumented: true) },
        { 0x83, new InstructionTemplate("RES 0,(<xy><d>),E", IsUndocumented: true) },
        { 0x84, new InstructionTemplate("RES 0,(<xy><d>),H", IsUndocumented: true) },
        { 0x85, new InstructionTemplate("RES 0,(<xy><d>),L", IsUndocumented: true) },
        { 0x86, new InstructionTemplate("RES 0,(<xy><d>)") },
        { 0x87, new InstructionTemplate("RES 0,(<xy><d>),A", IsUndocumented: true) },
        { 0x88, new InstructionTemplate("RES 1,(<xy><d>),B", IsUndocumented: true) },
        { 0x89, new InstructionTemplate("RES 1,(<xy><d>),C", IsUndocumented: true) },
        { 0x8A, new InstructionTemplate("RES 1,(<xy><d>),D", IsUndocumented: true) },
        { 0x8B, new InstructionTemplate("RES 1,(<xy><d>),E", IsUndocumented: true) },
        { 0x8C, new InstructionTemplate("RES 1,(<xy><d>),H", IsUndocumented: true) },
        { 0x8D, new InstructionTemplate("RES 1,(<xy><d>),L", IsUndocumented: true) },
        { 0x8E, new InstructionTemplate("RES 1,(<xy><d>)") },
        { 0x8F, new InstructionTemplate("RES 1,(<xy><d>),A", IsUndocumented: true) },
        { 0x90, new InstructionTemplate("RES 2,(<xy><d>),B", IsUndocumented: true) },
        { 0x91, new InstructionTemplate("RES 2,(<xy><d>),C", IsUndocumented: true) },
        { 0x92, new InstructionTemplate("RES 2,(<xy><d>),D", IsUndocumented: true) },
        { 0x93, new InstructionTemplate("RES 2,(<xy><d>),E", IsUndocumented: true) },
        { 0x94, new InstructionTemplate("RES 2,(<xy><d>),H", IsUndocumented: true) },
        { 0x95, new InstructionTemplate("RES 2,(<xy><d>),L", IsUndocumented: true) },
        { 0x96, new InstructionTemplate("RES 2,(<xy><d>)") },
        { 0x97, new InstructionTemplate("RES 2,(<xy><d>),A", IsUndocumented: true) },
        { 0x98, new InstructionTemplate("RES 3,(<xy><d>),B", IsUndocumented: true) },
        { 0x99, new InstructionTemplate("RES 3,(<xy><d>),C", IsUndocumented: true) },
        { 0x9A, new InstructionTemplate("RES 3,(<xy><d>),D", IsUndocumented: true) },
        { 0x9B, new InstructionTemplate("RES 3,(<xy><d>),E", IsUndocumented: true) },
        { 0x9C, new InstructionTemplate("RES 3,(<xy><d>),H", IsUndocumented: true) },
        { 0x9D, new InstructionTemplate("RES 3,(<xy><d>),L", IsUndocumented: true) },
        { 0x9E, new InstructionTemplate("RES 3,(<xy><d>)") },
        { 0x9F, new InstructionTemplate("RES 3,(<xy><d>),A", IsUndocumented: true) },
        { 0xA0, new InstructionTemplate("RES 4,(<xy><d>),B", IsUndocumented: true) },
        { 0xA1, new InstructionTemplate("RES 4,(<xy><d>),C", IsUndocumented: true) },
        { 0xA2, new InstructionTemplate("RES 4,(<xy><d>),D", IsUndocumented: true) },
        { 0xA3, new InstructionTemplate("RES 4,(<xy><d>),E", IsUndocumented: true) },
        { 0xA4, new InstructionTemplate("RES 4,(<xy><d>),H", IsUndocumented: true) },
        { 0xA5, new InstructionTemplate("RES 4,(<xy><d>),L", IsUndocumented: true) },
        { 0xA6, new InstructionTemplate("RES 4,(<xy><d>)") },
        { 0xA7, new InstructionTemplate("RES 4,(<xy><d>),A", IsUndocumented: true) },
        { 0xA8, new InstructionTemplate("RES 5,(<xy><d>),B", IsUndocumented: true) },
        { 0xA9, new InstructionTemplate("RES 5,(<xy><d>),C", IsUndocumented: true) },
        { 0xAA, new InstructionTemplate("RES 5,(<xy><d>),D", IsUndocumented: true) },
        { 0xAB, new InstructionTemplate("RES 5,(<xy><d>),E", IsUndocumented: true) },
        { 0xAC, new InstructionTemplate("RES 5,(<xy><d>),H", IsUndocumented: true) },
        { 0xAD, new InstructionTemplate("RES 5,(<xy><d>),L", IsUndocumented: true) },
        { 0xAE, new InstructionTemplate("RES 5,(<xy><d>)") },
        { 0xAF, new InstructionTemplate("RES 5,(<xy><d>),A", IsUndocumented: true) },
        { 0xB0, new InstructionTemplate("RES 6,(<xy><d>),B", IsUndocumented: true) },
        { 0xB1, new InstructionTemplate("RES 6,(<xy><d>),C", IsUndocumented: true) },
        { 0xB2, new InstructionTemplate("RES 6,(<xy><d>),D", IsUndocumented: true) },
        { 0xB3, new InstructionTemplate("RES 6,(<xy><d>),E", IsUndocumented: true) },
        { 0xB4, new InstructionTemplate("RES 6,(<xy><d>),H", IsUndocumented: true) },
        { 0xB5, new InstructionTemplate("RES 6,(<xy><d>),L", IsUndocumented: true) },
        { 0xB6, new InstructionTemplate("RES 6,(<xy><d>)") },
        { 0xB7, new InstructionTemplate("RES 6,(<xy><d>),A", IsUndocumented: true) },
        { 0xB8, new InstructionTemplate("RES 7,(<xy><d>),B", IsUndocumented: true) },
        { 0xB9, new InstructionTemplate("RES 7,(<xy><d>),C", IsUndocumented: true) },
        { 0xBA, new InstructionTemplate("RES 7,(<xy><d>),D", IsUndocumented: true) },
        { 0xBB, new InstructionTemplate("RES 7,(<xy><d>),E", IsUndocumented: true) },
        { 0xBC, new InstructionTemplate("RES 7,(<xy><d>),H", IsUndocumented: true) },
        { 0xBD, new InstructionTemplate("RES 7,(<xy><d>),L", IsUndocumented: true) },
        { 0xBE, new InstructionTemplate("RES 7,(<xy><d>)") },
        { 0xBF, new InstructionTemplate("RES 7,(<xy><d>),A", IsUndocumented: true) },
        { 0xC0, new InstructionTemplate("SET 0,(<xy><d>),B", IsUndocumented: true) },
        { 0xC1, new InstructionTemplate("SET 0,(<xy><d>),C", IsUndocumented: true) },
        { 0xC2, new InstructionTemplate("SET 0,(<xy><d>),D", IsUndocumented: true) },
        { 0xC3, new InstructionTemplate("SET 0,(<xy><d>),E", IsUndocumented: true) },
        { 0xC4, new InstructionTemplate("SET 0,(<xy><d>),H", IsUndocumented: true) },
        { 0xC5, new InstructionTemplate("SET 0,(<xy><d>),L", IsUndocumented: true) },
        { 0xC6, new InstructionTemplate("SET 0,(<xy><d>)") },
        { 0xC7, new InstructionTemplate("SET 0,(<xy><d>),A", IsUndocumented: true) },
        { 0xC8, new InstructionTemplate("SET 1,(<xy><d>),B", IsUndocumented: true) },
        { 0xC9, new InstructionTemplate("SET 1,(<xy><d>),C", IsUndocumented: true) },
        { 0xCA, new InstructionTemplate("SET 1,(<xy><d>),D", IsUndocumented: true) },
        { 0xCB, new InstructionTemplate("SET 1,(<xy><d>),E", IsUndocumented: true) },
        { 0xCC, new InstructionTemplate("SET 1,(<xy><d>),H", IsUndocumented: true) },
        { 0xCD, new InstructionTemplate("SET 1,(<xy><d>),L", IsUndocumented: true) },
        { 0xCE, new InstructionTemplate("SET 1,(<xy><d>)") },
        { 0xCF, new InstructionTemplate("SET 1,(<xy><d>),A", IsUndocumented: true) },
        { 0xD0, new InstructionTemplate("SET 2,(<xy><d>),B", IsUndocumented: true) },
        { 0xD1, new InstructionTemplate("SET 2,(<xy><d>),C", IsUndocumented: true) },
        { 0xD2, new InstructionTemplate("SET 2,(<xy><d>),D", IsUndocumented: true) },
        { 0xD3, new InstructionTemplate("SET 2,(<xy><d>),E", IsUndocumented: true) },
        { 0xD4, new InstructionTemplate("SET 2,(<xy><d>),H", IsUndocumented: true) },
        { 0xD5, new InstructionTemplate("SET 2,(<xy><d>),L", IsUndocumented: true) },
        { 0xD6, new InstructionTemplate("SET 2,(<xy><d>)") },
        { 0xD7, new InstructionTemplate("SET 2,(<xy><d>),A", IsUndocumented: true) },
        { 0xD8, new InstructionTemplate("SET 3,(<xy><d>),B", IsUndocumented: true) },
        { 0xD9, new InstructionTemplate("SET 3,(<xy><d>),C", IsUndocumented: true) },
        { 0xDA, new InstructionTemplate("SET 3,(<xy><d>),D", IsUndocumented: true) },
        { 0xDB, new InstructionTemplate("SET 3,(<xy><d>),E", IsUndocumented: true) },
        { 0xDC, new InstructionTemplate("SET 3,(<xy><d>),H", IsUndocumented: true) },
        { 0xDD, new InstructionTemplate("SET 3,(<xy><d>),L", IsUndocumented: true) },
        { 0xDE, new InstructionTemplate("SET 3,(<xy><d>)") },
        { 0xDF, new InstructionTemplate("SET 3,(<xy><d>),A", IsUndocumented: true) },
        { 0xE0, new InstructionTemplate("SET 4,(<xy><d>),B", IsUndocumented: true) },
        { 0xE1, new InstructionTemplate("SET 4,(<xy><d>),C", IsUndocumented: true) },
        { 0xE2, new InstructionTemplate("SET 4,(<xy><d>),D", IsUndocumented: true) },
        { 0xE3, new InstructionTemplate("SET 4,(<xy><d>),E", IsUndocumented: true) },
        { 0xE4, new InstructionTemplate("SET 4,(<xy><d>),H", IsUndocumented: true) },
        { 0xE5, new InstructionTemplate("SET 4,(<xy><d>),L", IsUndocumented: true) },
        { 0xE6, new InstructionTemplate("SET 4,(<xy><d>)") },
        { 0xE7, new InstructionTemplate("SET 4,(<xy><d>),A", IsUndocumented: true) },
        { 0xE8, new InstructionTemplate("SET 5,(<xy><d>),B", IsUndocumented: true) },
        { 0xE9, new InstructionTemplate("SET 5,(<xy><d>),C", IsUndocumented: true) },
        { 0xEA, new InstructionTemplate("SET 5,(<xy><d>),D", IsUndocumented: true) },
        { 0xEB, new InstructionTemplate("SET 5,(<xy><d>),E", IsUndocumented: true) },
        { 0xEC, new InstructionTemplate("SET 5,(<xy><d>),H", IsUndocumented: true) },
        { 0xED, new InstructionTemplate("SET 5,(<xy><d>),L", IsUndocumented: true) },
        { 0xEE, new InstructionTemplate("SET 5,(<xy><d>)") },
        { 0xEF, new InstructionTemplate("SET 5,(<xy><d>),A", IsUndocumented: true) },
        { 0xF0, new InstructionTemplate("SET 6,(<xy><d>),B", IsUndocumented: true) },
        { 0xF1, new InstructionTemplate("SET 6,(<xy><d>),C", IsUndocumented: true) },
        { 0xF2, new InstructionTemplate("SET 6,(<xy><d>),D", IsUndocumented: true) },
        { 0xF3, new InstructionTemplate("SET 6,(<xy><d>),E", IsUndocumented: true) },
        { 0xF4, new InstructionTemplate("SET 6,(<xy><d>),H", IsUndocumented: true) },
        { 0xF5, new InstructionTemplate("SET 6,(<xy><d>),L", IsUndocumented: true) },
        { 0xF6, new InstructionTemplate("SET 6,(<xy><d>)") },
        { 0xF7, new InstructionTemplate("SET 6,(<xy><d>),A", IsUndocumented: true) },
        { 0xF8, new InstructionTemplate("SET 7,(<xy><d>),B", IsUndocumented: true) },
        { 0xF9, new InstructionTemplate("SET 7,(<xy><d>),C", IsUndocumented: true) },
        { 0xFA, new InstructionTemplate("SET 7,(<xy><d>),D", IsUndocumented: true) },
        { 0xFB, new InstructionTemplate("SET 7,(<xy><d>),E", IsUndocumented: true) },
        { 0xFC, new InstructionTemplate("SET 7,(<xy><d>),H", IsUndocumented: true) },
        { 0xFD, new InstructionTemplate("SET 7,(<xy><d>),L", IsUndocumented: true) },
        { 0xFE, new InstructionTemplate("SET 7,(<xy><d>)") },
        { 0xFF, new InstructionTemplate("SET 7,(<xy><d>),A", IsUndocumented: true) },
    };
}