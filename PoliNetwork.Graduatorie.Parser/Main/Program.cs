#region

using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Utils;
using PoliNetwork.Graduatorie.Parser.Utils.Output;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = new ArgsConfig(args);
        argsConfig.Print();

        var rankingsUrls = Scraper.Main.Program.RankingsUrls(mt, argsConfig);

        // esegui ciò che fa il parser (parse + write)
        RunParser(argsConfig, rankingsUrls);
    }

    private static void RunParser(ArgsConfig argsConfig, IEnumerable<RankingUrl> rankingsUrls)
    {
        // ricava un unico set partendo dai file html salvati, dagli url 
        // trovati e dal precedente set salvato nel .json
        var parser = new Utils.Transformer.ParserNS.Parser(argsConfig);
        var rankingsSet = parser.GetRankings(rankingsUrls.ToList());

        var dateFound = DateFoundUtil.GetDateFound(argsConfig, rankingsSet);

        // salvare il set
        new OutputWriteUtil(argsConfig).SaveOutputs(rankingsSet, dateFound);
    }
}