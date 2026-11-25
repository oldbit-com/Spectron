using System.ComponentModel.DataAnnotations;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Debugger.Tests.ViewModels;

public class MemoryViewModelTests : IDisposable
{
    private readonly Emulator _emulator;
    private readonly MemoryViewModel _viewModel;
    private readonly List<Word> _goToAddresses = [];
    private readonly List<(Word Address, int Length)> _selections = [];

    public MemoryViewModelTests()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        _emulator = provider.GetRequiredService<EmulatorFactory>().Create(ComputerType.Spectrum48K, RomType.Original);

        WriteMemory(0x8000, 0xDE, 0xAD, 0xBE, 0xEF);
        WriteMemory(0x8100, 0xCA, 0xFE, 0xBA, 0xBE);
        WriteMemory(0x9000, 0xDE, 0xAD, 0xBE, 0xEF);
        WriteMemory(0xA000, 0xCA, 0xFE, 0xBA, 0xBE);

        _viewModel = new MemoryViewModel(_emulator)
        {
            GoTo = address => _goToAddresses.Add(address),
            Select = (address, length) => _selections.Add((address, length))
        };
    }

    [Theory]
    [InlineData(Key.D1)]
    [InlineData(Key.D3)]
    [InlineData(Key.A)]
    [InlineData(Key.Space)]
    public void Immediate_WhenNonEnterKeyPressed_ShouldNotExecuteCommand(Key key)
    {
        _viewModel.CommandText = "g 123";

        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = key });

        _goToAddresses.ShouldBeEmpty();
        _viewModel.CommandText.ShouldBe("g 123");
    }

    [Fact]
    public void Immediate_WhenEnterPressed_ShouldExecuteGoToAndClearCommand()
    {
        _viewModel.CommandText = "g 1234";

        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });

        _goToAddresses.ShouldBe([(Word)0x1234]);
        _viewModel.CommandText.ShouldBeEmpty();
    }

    [Fact]
    public void Immediate_WhenWriteCommandTyped_ShouldNotWriteUntilEnterPressed()
    {
        _viewModel.CommandText = "w 7000,1";

        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.D2 });
        _emulator.Memory.Read(0x7000).ShouldBe((byte)0);

        _viewModel.CommandText = "w 7000,12";
        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });
        _emulator.Memory.Read(0x7000).ShouldBe((byte)0x12);
    }

    [Fact]
    public void Immediate_WhenHistoryRecalled_ShouldNotExecuteCommand()
    {
        _viewModel.CommandText = "g 4000";
        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });

        _viewModel.CommandText = "g 5000";
        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });

        _goToAddresses.Clear();

        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Up });
        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Up });

        _goToAddresses.ShouldBeEmpty();
        _viewModel.CommandText.ShouldBe("g 4000");
    }

    [Theory]
    [InlineData("g 1234")]
    [InlineData("G 1234")]
    [InlineData("g 0x1234")]
    [InlineData("w 4000,12")]
    [InlineData("w 4000 , 12")]
    [InlineData("f DE AD")]
    [InlineData("f hello")]
    [InlineData("f \"hello world\"")]
    [InlineData("f \"\u00A3\u2588\"")]
    public void ValidateCommand_WhenCommandIsValid_ShouldSucceed(string command)
    {
        Validate(command).ShouldBe(ValidationResult.Success);
    }

    [Theory]
    [InlineData("g", "Invalid address")]
    [InlineData("g 10000", "Invalid address")]
    [InlineData("g1234", "Invalid command")]
    [InlineData("goto 1234", "Invalid command")]
    [InlineData("fill", "Invalid command")]
    [InlineData("wat", "Invalid command")]
    [InlineData("w 4000", "Invalid arguments")]
    [InlineData("w 4000,100", "Invalid value")]
    [InlineData("f", "Empty search text")]
    [InlineData("f \"\"", "Empty search text")]
    [InlineData("f \"", "Missing closing quote")]
    [InlineData("f \"abc", "Missing closing quote")]
    [InlineData("f \"\u017C\"", "Unsupported character")]
    public void ValidateCommand_WhenCommandIsInvalid_ShouldReturnError(string command, string error)
    {
        Validate(command).ShouldNotBeNull().ErrorMessage.ShouldBe(error);
    }

    [Fact]
    public void Immediate_WhenInvalidCommandTyped_ShouldShowErrorOnlyAfterEnter()
    {
        _viewModel.CommandText = "g";
        _viewModel.HasErrors.ShouldBeFalse();

        PressEnter();
        _viewModel.HasErrors.ShouldBeTrue();

        _viewModel.CommandText = "g 1";
        _viewModel.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void Immediate_WhenSameGoToRepeated_ShouldClearCommandEachTime()
    {
        Execute("g 4000");
        _viewModel.CommandText.ShouldBeEmpty();

        Execute("g 4000");
        _viewModel.CommandText.ShouldBeEmpty();

        _goToAddresses.ShouldBe([(Word)0x4000, (Word)0x4000]);
    }

    [Fact]
    public void Immediate_WhenFindRepeated_ShouldFindNextAndWrapAround()
    {
        Execute("f DE AD BE EF");
        PressEnter();
        PressEnter();

        _selections.ShouldBe([((Word)0x8000, 4), ((Word)0x9000, 4), ((Word)0x8000, 4)]);
        _viewModel.CommandText.ShouldBe("f DE AD BE EF");
    }

    [Fact]
    public void Immediate_WhenFindTextChanges_ShouldSearchFromStart()
    {
        Execute("f DE AD BE EF");
        PressEnter();

        Execute("f CA FE BA BE");

        _selections.ShouldBe([((Word)0x8000, 4), ((Word)0x9000, 4), ((Word)0x8100, 4)]);
    }

    [Fact]
    public void Immediate_WhenFindText_ShouldSearchZxCharacters()
    {
        Execute("f \"\u00A9\u2588\"");

        _selections.ShouldBeEmpty();

        WriteMemory(0xB000, 0x7F, 0x8F);
        _viewModel.Update(_emulator.Memory);
        PressEnter();

        _selections.ShouldBe([((Word)0xB000, 2)]);
    }

    private void Execute(string command)
    {
        _viewModel.CommandText = command;
        PressEnter();
    }

    private void PressEnter() => _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });

    private void WriteMemory(Word address, params byte[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            _emulator.Memory.Write((Word)(address + i), values[i]);
        }
    }

    private static ValidationResult? Validate(string command) =>
        MemoryViewModel.ValidateCommand(command, new ValidationContext(command));

    public void Dispose() => _viewModel.Dispose();
}
