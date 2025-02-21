namespace OldBit.Spectron.Recorder;

internal static class FileHelper
{
    internal static bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal static bool TryDeleteFolder(DirectoryInfo? directory)
    {
        if (directory == null)
        {
            return true;
        }

        try
        {
            directory.Delete(true);
        }
        catch
        {
            return false;
        }

        return true;
    }

    internal static bool TryMoveFile(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            File.Move(sourceFilePath, destinationFilePath, overwrite: true);
        }
        catch
        {
            return false;
        }

        return true;
    }
}