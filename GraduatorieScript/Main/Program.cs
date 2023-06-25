using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;
using SampleNuGet.Utils;

namespace GraduatorieScript.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var docsFolder = GetDocsFolder(args);
        Console.WriteLine($"[INFO] baseFolder: {docsFolder}");

        if (string.IsNullOrEmpty(docsFolder))
        {
            Console.WriteLine("[INFO] baseFolder is null. Abort.");
            return;
        }

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        ScraperOutput.Write(rankingsUrls, docsFolder);

        //print links found
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");

        // ricava un unico set partendo dai file html salvati, dagli url trovati e
        // dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(docsFolder, rankingsUrls);
        MainJson.Write(docsFolder, rankingsSet);

        //eliminare i suddetti file html
        /* if (transformerResult?.pathFound != null) */
        /*     FileUtils.TryBulkDelete(transformerResult.pathFound); */
        // ^^ this must be wrong
    }

    private static string GetDocsFolder(IReadOnlyList<string> args)
    {
        var folder = args.Count > 0 ? args[0] : null;
        var b = args.Count > 0 && !string.IsNullOrEmpty(folder);
        var docsFolder = b
            ? folder
            : PathUtils.FindFolder(Constants.FolderToFind);

        return !string.IsNullOrEmpty(docsFolder)
            ? docsFolder
            : PathUtils.CreateAndReturnDocsFolder(Constants.FolderToFind);
    }
}
