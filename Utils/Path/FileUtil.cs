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
            ;
        }
    }
}