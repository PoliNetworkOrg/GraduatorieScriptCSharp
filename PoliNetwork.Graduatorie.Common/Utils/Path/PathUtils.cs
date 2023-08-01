namespace PoliNetwork.Graduatorie.Common.Utils.Path;

public static class PathUtils
{
    public static string? FindFolder(string folderToFind)
    {
        return FindFolder(Directory.GetCurrentDirectory(), folderToFind);
    }

    private static string? FindFolder(string? startingFolder, string folderToFind)
    {
        while (true)
        {
            if (string.IsNullOrEmpty(startingFolder))
                return null;

            // Constants

            // Check if the starting folder itself is the data folder
            var findDataFolder = System.IO.Path.Combine(startingFolder, folderToFind);

            if (Directory.Exists(findDataFolder)) return findDataFolder;

            // Get the subdirectories in the starting folder
            string?[] subdirectories = Directory.GetDirectories(startingFolder);

            // Iterate through the subdirectories
            return subdirectories.Select(x => FindFolder(x, folderToFind))
                .FirstOrDefault(dataFolder => !string.IsNullOrEmpty(dataFolder));
        }
    }

    public static string CreateAndReturnDataFolder(string folderName)
    {
        var pwd = Directory.GetCurrentDirectory();
        var dataFolderPath = System.IO.Path.Join(pwd, folderName);
        Directory.CreateDirectory(dataFolderPath);
        return dataFolderPath;
    }
}
