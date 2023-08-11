#region

using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public static class ScraperOutput
{
    private static string GetFilePath(string? docFolder)
    {
        return docFolder + "/" + Constants.OutputLinksFilename;
    }

    public static List<RankingUrl> GetWithUrlsFromLocalFileLinks(IEnumerable<RankingUrl> urls, string? dataFolder)
    {
        var links = GetSaved(dataFolder);
        links.AddRange(urls);
        return Distinct(links);
    }

    private static List<RankingUrl> Distinct(IEnumerable<RankingUrl> links)
    {
        var list = new List<RankingUrl>();
        var rankingUrls = links.Where(variable => list.All(x => x.Url != variable.Url));
        list.AddRange(rankingUrls);
        return list;
    }

    private static List<RankingUrl> GetSaved(string? dataFolder)
    {
        List<RankingUrl> list = new();
        var filePath = GetFilePath(dataFolder);
        if (!File.Exists(filePath)) return list;
        try
        {
            var lines = File.ReadAllLines(filePath);
            var rankingUrls = from line in lines where !string.IsNullOrEmpty(line) select RankingUrl.From(line);
            list.AddRange(rankingUrls);

            return list;
        }
        catch
        {
            // consider to handle them
            Console.WriteLine($"[ERROR] Can't read the ScraperOutput file ({filePath})");
            return list;
        }
    }

    public static void Write(List<RankingUrl> rankingsUrls, string? dataFolder)
    {
        if (string.IsNullOrEmpty(dataFolder))
            return;

        var filePath = GetFilePath(dataFolder);

        var output = GetOutputLinksString(rankingsUrls);

        Console.WriteLine($"[INFO] ScraperOutput writing to file {filePath}: {rankingsUrls.Count} links");
        File.WriteAllText(filePath, output);
    }

    private static string GetOutputLinksString(IEnumerable<RankingUrl> rankingsUrls)
    {
        var output = "";
        var urls = CheckUrlUtil.GetRankingLinksHashSet(rankingsUrls).Order();
        foreach (var link in urls)
        {
            output += link;
            output += "\n";
        }

        return output;
    }
}