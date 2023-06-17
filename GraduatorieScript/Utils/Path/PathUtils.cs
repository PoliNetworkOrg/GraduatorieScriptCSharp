namespace GraduatorieScript.Utils.Path;

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


            // Check if the starting folder itself is the "docs" folder
            var findDocsFolder = System.IO.Path.Combine(startingFolder, folderToFind);

            if (Directory.Exists(findDocsFolder)) return findDocsFolder;

            // Get the subdirectories in the starting folder
            string?[] subdirectories = Directory.GetDirectories(startingFolder);

            // Iterate through the subdirectories
            return subdirectories.Select(x => FindFolder(x, folderToFind))
                .FirstOrDefault(docsFolder => !string.IsNullOrEmpty(docsFolder));
        }
    }

    public static string CreateAndReturnDocsFolder(string folderToFind)
    {
        var s = Directory.GetCurrentDirectory();
        var andReturnDocsFolder = System.IO.Path.Join(s, folderToFind);
        Directory.CreateDirectory(andReturnDocsFolder);
        return andReturnDocsFolder;
    }
}