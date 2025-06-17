using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging.Messages;
using OldBit.Spectron.Emulation.Files;

namespace OldBit.Spectron.Messages;

public class ShowSelectArchiveFileViewMessage(List<ArchiveEntry> fileNames) : AsyncRequestMessage<ArchiveEntry?>
{
    public List<ArchiveEntry> FileNames { get; } = fileNames;
}