## Overview

Spectron is a ZX Spectrum emulator built with C# and Avalonia UI.
The project targets cross-platform support (Windows, macOS, Linux) 
and focuses on accurate emulation and a modern user interface.

## Tech Stack
- **Language:** C# (.NET 10)
- **UI Framework:** Avalonia UI 12
- **Platforms:** Windows, macOS, Linux
- **Architecture:** MVVM (using CommunityToolkit.Mvvm)

## Build & Run
```bash
dotnet build
dotnet run --project src/Spectron/Spectron.csproj
```
## Tests
```bash
dotnet test
```

## Project Structure
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
└── /Spectron.Tests                 # Tests for the UI related components
```

## Core Dependencies
| Package                 | Purpose                                  |
|-------------------------|------------------------------------------|
| Avalonia UI             | Cross-platform UI framework              |
| CommunityToolkit.Mvvm   | MVVM source generators and base classes  |
| OldBit.Z80Cpu.Spectron  | Z80 CPU emulation                        |
| OldBit.Beep             | Audio playback, custom package           |
| OldBit.Spectron.Files   | TZX, TAP, Z80 etc file support           |

## Architecture & Patterns

### MVVM with CommunityToolkit.Mvvm
- Use [ObservableProperty], [RelayCommand], and ObservableObject / ObservableValidator.
- Property change side-effects are handled via partial On<PropertyName>Changed methods.
- Communication between ViewModels often uses WeakReferenceMessenger.

### Core Emulation (Spectron.Emulation)
- Z80 CPU: Provided by OldBit.Z80Cpu. Use Word (alias for ushort) for 16-bit addresses.
- SpectrumBus: Implements IBus and manages communication between the CPU and devices (IDevice).
- Devices: Hardware components (e.g., Ula, Memory48K, AyDevice) implement IDevice and provide ReadPort / WritePort or memory access logic.
- Timing: Emulation is frame-based. Hardware settings (ticks per frame, contention, etc.) are defined in Hardware.cs.

### UI Composition
- Large ViewModels like MainWindowViewModel are split into multiple partial files located in the ViewModels directory
(e.g., MainWindowViewModel.Emulator.cs, MainWindowViewModel.Display.cs).
- Dependency Injection (DI) is used for service and ViewModel registration.

### Platform-specific patterns
For macOS native menu is implented in src/Spectron/Controls/NativeMainMenu.cs.

## Coding Guidelines
- Modern C#: Prefer var over explicit types.
- Naming: Use meaningful variable and method names following standard .NET conventions (PascalCase for methods/properties, camelCase for private fields with _ prefix).
- Global Usings: Word is defined as a global using for ushort in the Emulation project.

## Testing Guidelines
- All internal classes and methods are accessible in test projects, no need to change access modifiers.
- Use NSubstitute only if easy to mock. In most cases use actual objects for testing.
- Use Shouldly for assertions.
- xunit.v3 is used for unit tests ([Fact], [Theory], [InlineData]).
- Do not add `// Arrange` `// Act` `// Assert` comments to tests.

## Guidelines for AI Agents
- NuGet Dependencies: Do not introduce new NuGet dependencies without explicit approval.
- File Organization: When adding functionality to ViewModels, check if a partial class structure is already in place.
- Emulation Safety: Core emulation logic in Spectron.Emulation is performance-sensitive. Avoid heavy allocations in hot paths (like RunFrame or ReadPort).
