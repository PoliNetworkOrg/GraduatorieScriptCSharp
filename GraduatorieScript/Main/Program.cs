using GraduatorieScript.Data.Constants;
using GraduatorieScript.Objects.Json.Indexes;
using GraduatorieScript.Objects.Json.Stats;
using GraduatorieScript.Objects.RankingNS;
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

        ArgsConfig argsConfig = GetArgsConfig(args);
        Console.WriteLine($"[INFO] dataFolder: {argsConfig.dataFolder}");

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        rankingsUrls = ScraperOutput.GetWithUrlsFromLocalFileLinks(rankingsUrls, argsConfig.dataFolder);
        ScraperOutput.Write(rankingsUrls, argsConfig.dataFolder);

        //print links found
        PrintLinks(rankingsUrls);

        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(argsConfig.dataFolder, rankingsUrls);

        // salvare il set
        SaveOutputs(argsConfig.dataFolder, rankingsSet);
    }

    private static ArgsConfig GetArgsConfig(string[] args)
    {
        throw new NotImplementedException();
    }

    private static void PrintLinks(List<RankingUrl> rankingsUrls)
    {
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");
    }

    private static void SaveOutputs(string? dataFolder, RankingsSet? rankingsSet)
    {
        if (string.IsNullOrEmpty(dataFolder))
            return;
        
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        IndexJsonBase.IndexesWrite(rankingsSet, outFolder);
        StatsJson.Write(outFolder, rankingsSet);
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

public class ArgsConfig
{
    public string? dataFolder;
}