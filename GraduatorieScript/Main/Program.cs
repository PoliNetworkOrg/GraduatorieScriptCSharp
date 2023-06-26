using GraduatorieScript.Data;
using GraduatorieScript.Objects.Json;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using SampleNuGet.Utils;

namespace GraduatorieScript.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var docsFolder = GetDocsFolder(args);
        Console.WriteLine($"[INFO] docsFolder: {docsFolder}");

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll);
        ScraperOutput.Write(rankingsUrls, docsFolder);

        //print links found
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");

        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(docsFolder, rankingsUrls);

        // salvare il set
        var outFolder = Path.Join(docsFolder, Constants.OutputFolder);
        MainJson.Write(outFolder, rankingsSet);
        StatsJson.Write(outFolder, rankingsSet);
    }

    private static string GetDocsFolder(IReadOnlyList<string> args)
    {
        // check if docsFolder passed in args
        var argsFolder = args.Count > 0 ? args[0] : null;

        // use it if passed or search the default
        var docsFolder = !string.IsNullOrEmpty(argsFolder)
            ? argsFolder
            : PathUtils.FindFolder(Constants.FolderToFind);

        // if not found, create it
        if (string.IsNullOrEmpty(docsFolder))
        {
            Console.WriteLine("[WARNING] docsFolder not found, creating it");
            return PathUtils.CreateAndReturnDocsFolder(Constants.FolderToFind);
        }

        return docsFolder;
    }
}
