namespace GraduatorieScript.Utils.Path;

public static class PathUtils
{
    public static string? FindDocsFolder()
    {
        return FindDocsFolder(Directory.GetCurrentDirectory());
    }

    private static string? FindDocsFolder(string? startingFolder)
    {
        while (true)
        {
            if (string.IsNullOrEmpty(startingFolder))
                return null;

            // Constants
            const string folderToFind = "docs";

            // Check if the starting folder itself is the "docs" folder
            var findDocsFolder = System.IO.Path.Combine(startingFolder, folderToFind);

            if (Directory.Exists(findDocsFolder)) return findDocsFolder;

            // Get the subdirectories in the starting folder
            string?[] subdirectories = Directory.GetDirectories(startingFolder);

            // Iterate through the subdirectories
            foreach (var subdirectory in subdirectories)
            {
                // Recursively search for the "docs" folder
                var docsFolder = FindDocsFolder(subdirectory);
                if (string.IsNullOrEmpty(docsFolder)) continue;
                return docsFolder;
            }

            // If the "docs" folder is not found in the starting folder return null
            return null;
        }
    }
}