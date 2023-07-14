using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Utils.Output;

namespace PoliNetwork.Graduatorie.Parser.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = ArgsConfig.GetArgsConfig(args);
        argsConfig.Print();

        var rankingsUrls = Scraper.Main.Program.RankingsUrls(mt, argsConfig);

        // esegui ciò che fa il parser (parse + write)
        ParserDo(argsConfig, rankingsUrls);
    }

    private static void ParserDo(ArgsConfig argsConfig, IEnumerable<RankingUrl> rankingsUrls)
    {
        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var rankingsSet = Utils.Transformer.ParserNS.Parser.GetRankings(argsConfig, rankingsUrls);

        // salvare il set
        OutputWriteUtil.SaveOutputs(argsConfig.DataFolder, rankingsSet);
    }
}