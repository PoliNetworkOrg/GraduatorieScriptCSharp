using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Objects.Json.Indexes;
using GraduatorieScript.Objects.Json.Stats;
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

        var dataFolder = GetDataFolder(args);
        Console.WriteLine($"[INFO] dataFolder: {dataFolder}");

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        rankingsUrls = ScraperOutput.GetWithUrlsFromLocalFileLinks(rankingsUrls, dataFolder);
        ScraperOutput.Write(rankingsUrls, dataFolder);

        //print links found
        PrintLinks(rankingsUrls);

        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(dataFolder, rankingsUrls);

        // salvare il set
        SaveOutputs(dataFolder, rankingsSet);
    }

    private static void PrintLinks(List<RankingUrl> rankingsUrls)
    {
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");
    }

    private static void SaveOutputs(string dataFolder, RankingsSet rankingsSet)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        IndexesWrite(rankingsSet, outFolder);
        StatsJson.Write(outFolder, rankingsSet);
    }

    private static void IndexesWrite(RankingsSet rankingsSet, string outFolder)
    {
        IndexJsonBase.WriteSingleJsons(rankingsSet, outFolder);
        BySchoolYearJson.From(rankingsSet).WriteToFile(outFolder, Constants.MainJsonFilename);
    }

    private static string GetDataFolder(IReadOnlyList<string> args)
    {
        // check if dataFolder passed in args
        var argsFolder = args.Count > 0 ? args[0] : null;

        // use it if passed or search the default
        var dataFolder = !string.IsNullOrEmpty(argsFolder)
            ? argsFolder
            : PathUtils.FindFolder(Constants.DataFolder);


        if (!string.IsNullOrEmpty(dataFolder)) return dataFolder;

        // if not found, create it
        Console.WriteLine("[WARNING] dataFolder not found, creating it");
        return PathUtils.CreateAndReturnDataFolder(Constants.DataFolder);
    }
}