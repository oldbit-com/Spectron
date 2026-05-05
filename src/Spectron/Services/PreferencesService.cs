using System.Threading.Tasks;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Services;

public class PreferencesService(IApplicationDataService applicationDataService)
{
    public async Task SaveAsync(Preferences settings) => await applicationDataService.SaveAsync(settings);

    public async Task<Preferences> LoadAsync() => await applicationDataService.LoadAsync<Preferences>();
}