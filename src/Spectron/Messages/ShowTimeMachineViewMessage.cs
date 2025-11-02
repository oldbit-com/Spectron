using CommunityToolkit.Mvvm.Messaging.Messages;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Messages;

public class ShowTimeMachineViewMessage(TimeMachineViewModel viewModel) : AsyncRequestMessage<TimeMachineEntry?>
{
    public TimeMachineViewModel ViewModel { get; } = viewModel;
}