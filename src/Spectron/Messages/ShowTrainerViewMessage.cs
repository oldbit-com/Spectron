using OldBit.Spectron.Emulation;
using OldBit.Spectron.Files.Pok;

namespace OldBit.Spectron.Messages;

public record ShowTrainerViewMessage(Emulator Emulator, PokeFile? PokeFile);