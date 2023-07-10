using GraduatorieCommon.Utils.ParallelNS;
using GraduatorieScript.Data;
using GraduatorieScript.Objects.Json.Indexes;
using GraduatorieScript.Objects.Json.Stats;
using GraduatorieScript.Objects.RankingNS;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer.ParserNS;
using GraduatorieScript.Utils.Web;
using PoliNetwork.Core.Utils;

namespace GraduatorieScript.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = GetArgsConfig(args);
        Console.WriteLine($"[INFO] dataFolder: {argsConfig.DataFolder}");

        Console.WriteLine($"[INFO] thread max count: {ParallelRun.MaxDegreeOfParallelism}");

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        rankingsUrls = ScraperOutput.GetWithUrlsFromLocalFileLinks(rankingsUrls, argsConfig.DataFolder);
        ScraperOutput.Write(rankingsUrls, argsConfig.DataFolder);

        //print links found
        PrintLinks(rankingsUrls);

        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(argsConfig, rankingsUrls);

        // salvare il set
        SaveOutputs(argsConfig.DataFolder, rankingsSet);
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

    private static ArgsConfig GetArgsConfig(IReadOnlyList<string> args)
    {
        var argsConfig = new ArgsConfig
        {
            DataFolder = GetDataFolder(FindArgString(args, "--data")),
            ForceReparsing = FindArgPresent(args, "--reparse")
        };
        return argsConfig;
    }

    private static bool? FindArgPresent(IEnumerable<string> args, string reparse)
    {
        return args.Any(x => x == reparse);
    }

    private static string? FindArgString(IReadOnlyList<string> args, string data)
    {
        for (var i = 0; i < args.Count; i++)
        {
            var s = args[i];
            if (s != data) continue;
            if (i + 1 < args.Count) return args[i + 1];
        }

        return null;
    }

    private static string GetDataFolder(string? argsFolder)
    {
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