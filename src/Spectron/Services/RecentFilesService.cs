using System.Threading.Tasks;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Services;

public class RecentFilesService(ApplicationDataService applicationDataService)
{
    public async Task SaveAsync(object settings) => await applicationDataService.SaveAsync(settings);

    public async Task<RecentFilesSettings> LoadAsync() => await applicationDataService.LoadAsync<RecentFilesSettings>();
}