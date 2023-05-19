namespace GraduatorieScript.Utils.Path;

public static class FileUtil
{
    public static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    public static void DeleteFiles(List<string>? transformerResultPathFound)
    {
        if (transformerResultPathFound == null) return;
        foreach (var toDelete in transformerResultPathFound)
        {
            TryDelete(toDelete);
        }
    }
}