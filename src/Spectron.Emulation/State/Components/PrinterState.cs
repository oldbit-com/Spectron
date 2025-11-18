using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record PrinterState(bool IsZxPrinterEnabled);