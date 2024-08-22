#region

using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Scraper.Utils.Web;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = new ArgsConfig(args);
        argsConfig.Print();

        RankingsUrls(mt, argsConfig);
    }


    public static List<RankingUrl> RankingsUrls(Metrics mt, ArgsConfig argsConfig)
    {
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        rankingsUrls = ScraperOutput.GetWithUrlsFromLocalFileLinks(rankingsUrls, argsConfig.DataFolder);

        var scraper = new Utils.Web.Scraper();
        var manifesti = mt.Execute(scraper.ScrapeManifesti);

        PrintLinks(rankingsUrls);
        // save results
        ScraperOutput.WriteLinks(rankingsUrls, argsConfig.DataFolder);
        ScraperOutput.WriteManifesti(manifesti, argsConfig.DataFolder);

        return rankingsUrls;
    }

    private static void PrintLinks(List<RankingUrl> rankingsUrls)
    {
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");
    }
}