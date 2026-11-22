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

    public MemoryViewModelTests()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        _emulator = provider.GetRequiredService<EmulatorFactory>().Create(ComputerType.Spectrum48K, RomType.Original);
        _viewModel = new MemoryViewModel(_emulator)
        {
            GoTo = address => _goToAddresses.Add(address)
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
        _viewModel.CommandText = "w 8000,1";

        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.D2 });
        _emulator.Memory.Read(0x8000).ShouldBe((byte)0);

        _viewModel.CommandText = "w 8000,12";
        _viewModel.ImmediateCommand.Execute(new KeyEventArgs { Key = Key.Enter });
        _emulator.Memory.Read(0x8000).ShouldBe((byte)0x12);
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

    public void Dispose() => _viewModel.Dispose();
}
