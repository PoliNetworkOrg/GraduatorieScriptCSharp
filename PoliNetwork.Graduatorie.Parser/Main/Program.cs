using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Common.Utils.ParallelNS;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Scraper.Utils.Web;

namespace PoliNetwork.Graduatorie.Parser.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = ArgsConfig.GetArgsConfig(args);
        argsConfig.Print();

        var rankingsUrls = Scraper.Main.Program.RankingsUrls(mt, argsConfig);

        ParserDo(argsConfig, rankingsUrls);
    }

    private static void ParserDo(ArgsConfig argsConfig, List<RankingUrl> rankingsUrls)
    {
        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Utils.Transformer.ParserNS.Parser.GetRankings(argsConfig, rankingsUrls);

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


}