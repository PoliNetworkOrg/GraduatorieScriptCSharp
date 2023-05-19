namespace GraduatorieScript.Utils.Path;

public static class FileUtil
{
    public static void TryDelete(string? path)
    {
        try
        {
            if (path != null) 
                File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    public static void DeleteFiles(HashSet<string?>? transformerResultPathFound)
    {
        if (transformerResultPathFound == null) return;
        foreach (var toDelete in transformerResultPathFound)
        {
            TryDelete(toDelete);
        }
    }
}