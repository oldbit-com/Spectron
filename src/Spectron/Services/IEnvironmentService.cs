namespace OldBit.Spectron.Services;

public interface IEnvironmentService
{
    string GetAppDataPath(string fileName);
}