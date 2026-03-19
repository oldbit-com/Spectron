using System.Threading.Tasks;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Services;

public class FavoritesService(ApplicationDataService applicationDataService)
{
    public async Task SaveAsync(FavoritePrograms favorites) => await applicationDataService.SaveAsync(favorites);

    public async Task<FavoritePrograms> LoadAsync() => await applicationDataService.LoadAsync<FavoritePrograms>();
}