using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class TrainerView : ReactiveWindow<TrainerViewModel>
{
    public TrainerView() => InitializeComponent();
}