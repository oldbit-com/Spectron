using System.Threading.Tasks;

namespace OldBit.Spectron.Services;

public interface IApplicationDataService
{
    Task SaveAsync(object settings);

    Task<T> LoadAsync<T>() where T : new();
}