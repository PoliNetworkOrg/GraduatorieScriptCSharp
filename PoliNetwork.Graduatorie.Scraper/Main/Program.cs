using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Common.Utils.ParallelNS;
using PoliNetwork.Graduatorie.Scraper.Utils.Web;

namespace PoliNetwork.Graduatorie.Scraper.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var argsConfig = ArgsConfig.GetArgsConfig(args);
        Console.WriteLine($"[INFO] dataFolder: {argsConfig.DataFolder}");

        Console.WriteLine($"[INFO] thread max count: {ParallelRun.MaxDegreeOfParallelism}");

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        rankingsUrls = ScraperOutput.GetWithUrlsFromLocalFileLinks(rankingsUrls, argsConfig.DataFolder);
        ScraperOutput.Write(rankingsUrls, argsConfig.DataFolder);

        //print links found
        PrintLinks(rankingsUrls);
        
    }


    private static void PrintLinks(List<RankingUrl> rankingsUrls)
    {
        foreach (var r in rankingsUrls)
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");
    }

  
 
}