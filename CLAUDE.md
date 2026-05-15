# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Spectron is a ZX Spectrum emulator built with C# and Avalonia UI. The project targets cross-platform support (Windows, macOS, Linux)
and focuses on accurate emulation and a modern user interface.

## Commands

```bash
# Build
dotnet build Spectron.slnx
dotnet build src/Spectron/Spectron.csproj

# Run
dotnet run --project src/Spectron

# Test (all non-integration)
dotnet test --filter "Category!=Integration"

# Single test project
dotnet test tests/Spectron.Tests/Spectron.Tests.csproj

# Single test by name
dotnet test --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
```

Integration tests (`Category=Integration`) are excluded from CI and run actual emulator frames, 
then compare screen memory hashes against known-good results.

## Architecture

The solution (`Spectron.slnx`) has five source projects with a strict dependency hierarchy:

```
/src
├── /Spectron                       # UI / display rendering
│   ├── /ViewModels                 # MVVM ViewModels
│   ├── /Views                      # Avalonia XAML Views
│   ├── /Services                   # UI-related services (Settings, Session, etc.)
│   ├── /Platforms                  # Platform specific code (Interop)
│   └── /Controls                   # Custom Avalonia controls
├── /Spectron.Debugger              # Debugger UI
├── /Spectron.Emulation             # Core emulation logic
│   ├── /Devices                    # Virtual hardware (ULA, Memory, Bus, Audio, etc.)
│   ├── /Rom                        # Built-in ROM files and reader
│   ├── /Screen                     # Rendering and frame buffer logic
│   ├── /Snapshot                   # SNA, SZX, Z80 snapshot support
│   ├── /Tape                       # Cassette/Tape emulation
│   └── /TimeTravel                 # Time machine / re-play
├── /Spectron.Recorder              # Audio and video recorder
/tests
├── /Spectron.Debugger.Tests        # Tests for the debugger UI
├── /Spectron.Disassembly.Tests     # Tests for the Z80 disassembler
├── /Spectron.Emulator.Tests        # Tests for the core emulation logic
├── /Spectron.Integration.Tests     # Tests that execute real Spectrum software
└── /Spectron.Tests                 # Tests for the UI related components
```

**Spectron.Emulation** is the only project with no dependency on Avalonia. 
It contains the `Emulator` class, which orchestrates the Z80 CPU (`OldBit.Z80Cpu.Spectron` NuGet), ULA, and all device subsystems (audio, tape, disk, keyboard, memory, snapshot, time travel, etc.). 
The emulator fires `FrameCompleted` events that the UI consumes to render frames.

**Spectron** (the main app) uses Avalonia + `CommunityToolkit.Mvvm`. `MainViewModel` is split across 11 partial class files in `src/Spectron/ViewModels/`.
Each functional area (tape, disk, snapshots, audio, etc.) lives in its own partial file.

## Dependency Injection

`Program.cs` is the composition root. Three extension methods wire up the container:

- `services.AddEmulation()` — emulator, snapshot stores, time machine, managers
- `services.AddServices()` — UI services (preferences, favorites, recent files, sessions)
- `services.AddViewModels()` — all ViewModels as singletons

## Key Patterns

- **MVVM**: `ObservableObject`, `RelayCommand`, `[ObservableProperty]` from `CommunityToolkit.Mvvm`; messaging via `WeakReferenceMessenger` and types in `src/Spectron/Messages/`
- **Snapshot stores**: `IStateSnapshotStore<T>` / `StateSnapshotStore<T>` (generic) with separate implementations for quick-save and time-travel slots; stores live in `Spectron.Emulation/State/`
- **File formats**: snapshot loading/saving goes through `SnapshotManager`; formats include SNA, Z80, SZX, and `.spectron` (custom)
- **Serialization**: `MemoryPack` is used for fast binary state serialization in time-travel / quick-save

## Avalonia Notes

- Version: **12.x.x**

## Testing Notes

- All internal classes and methods are accessible in test projects, no need to change access modifiers.
- Use NSubstitute only if easy to mock. In most cases use actual objects for testing.
- Use Shouldly for assertions.
- xunit.v3 is used for unit tests ([Fact], [Theory], [InlineData]).
- Do not add `// Arrange` `// Act` `// Assert` comments to tests.

## Namespaces

| Project | Root namespace |
|---|---|
| Spectron | `OldBit.Spectron` |
| Spectron.Emulation | `OldBit.Spectron.Emulation` |
| Spectron.Debugger | `OldBit.Spectron.Debugger` |
| Spectron.Disassembly | `OldBit.Spectron.Disassembly` |
| Spectron.Recorder | `OldBit.Spectron.Recorder` |
