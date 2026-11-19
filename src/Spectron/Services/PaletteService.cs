using System.Threading.Tasks;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Services;

public class PaletteService(IApplicationDataService applicationDataService)
{
    public async Task SaveAsync(PaletteSettings paletteSettings) => await applicationDataService.SaveAsync(paletteSettings);

    public async Task<PaletteSettings> LoadAsync() => await applicationDataService.LoadAsync<PaletteSettings>();
}