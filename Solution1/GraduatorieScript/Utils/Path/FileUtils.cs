namespace GraduatorieScript.Utils.Path;

public static class FileUtils
{
    public static bool TryDelete(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        try
        {
            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void TryBulkDelete(IEnumerable<string?> paths)
    {
        foreach (var path in paths) TryDelete(path);
    }
}