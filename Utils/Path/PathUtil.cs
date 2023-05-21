namespace GraduatorieScript.Utils.Path;

public static class PathUtil
{
    public static string? FindDocsFolder()
    {
        return FindDocsFolder(Directory.GetCurrentDirectory(), true);
    }

    private static string? FindDocsFolder(string? startingFolder, bool callingForExploringUp)
    {
        while (true)
        {
            if (string.IsNullOrEmpty(startingFolder))
                return null;

            // Constants
            const string folderToFind = "docs";
            const string rankings = "rankings";

            // Check if the starting folder itself is the "docs" folder
            var findDocsFolder = System.IO.Path.Combine(startingFolder, folderToFind);

            if (Directory.Exists(findDocsFolder))
                if (findDocsFolder.ToLower().Contains(rankings))
                    return findDocsFolder;

            // Get the subdirectories in the starting folder
            string?[] subdirectories = Directory.GetDirectories(startingFolder);

            // Iterate through the subdirectories
            foreach (var subdirectory in subdirectories)
            {
                // Recursively search for the "docs" folder
                var docsFolder = FindDocsFolder(subdirectory, false);
                if (string.IsNullOrEmpty(docsFolder)) continue;
                if (docsFolder.ToLower().Contains(rankings))
                    return docsFolder;
            }

            if (callingForExploringUp)
            {
                // If the "docs" folder is not found in the starting folder or its subdirectories,
                // go one level up and try again
                var parentFolder = Directory.GetParent(startingFolder)?.FullName;
                if (string.IsNullOrEmpty(parentFolder)) return null;
                if (parentFolder == startingFolder) return null;
                startingFolder = parentFolder;
            }
            else
            {
                // If the "docs" folder is not found in the starting folder or any of its parent folders, return null
                return null;
            }
        }
    }
}