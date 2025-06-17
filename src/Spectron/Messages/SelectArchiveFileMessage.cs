using OldBit.Spectron.Emulation.Files;

namespace OldBit.Spectron.Messages;

public record SelectArchiveFileMessage(ArchiveEntry? SelectedFile);