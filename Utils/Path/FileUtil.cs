namespace GraduatorieScript.Utils.Path;

public class FileUtil
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