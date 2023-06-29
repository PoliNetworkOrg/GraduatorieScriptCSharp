using GraduatorieScript.Data.Constants;
using GraduatorieScript.Objects.RankingNS;

namespace GraduatorieScript.Utils.Web;

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
        var output = "";
        var rankingUrls = rankingsUrls.Where(UrlUtils.CheckUrl);
        foreach (var link in rankingUrls)
        {
            output += link.Url;
            output += "\n";
        }

        Console.WriteLine($"[INFO] ScraperOutput writing to file {filePath}: {rankingsUrls.Count} links");
        File.WriteAllText(filePath, output);
    }
}