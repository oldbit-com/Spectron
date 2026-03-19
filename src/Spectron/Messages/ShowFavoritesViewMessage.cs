using CommunityToolkit.Mvvm.Messaging.Messages;
using OldBit.Spectron.Settings;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Messages;

public class ShowFavoritesViewMessage(FavoritesViewModel viewModel) : AsyncRequestMessage<FavoritePrograms?>
{
    public FavoritesViewModel ViewModel { get; } = viewModel;
}