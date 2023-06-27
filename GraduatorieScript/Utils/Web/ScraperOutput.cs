using GraduatorieScript.Data;
using GraduatorieScript.Objects;

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

        links = Distinct(links);
        
        return links;


    }

    private static List<RankingUrl> Distinct(List<RankingUrl> links)
    {
        List<RankingUrl> list = new List<RankingUrl>();
        foreach (var variable in links)
        {
            if (list.All(x => x.Url != variable.Url))
                list.Add(variable);
        }
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
            foreach (var line in lines)
                if (!string.IsNullOrEmpty(line))
                    list.Add(RankingUrl.From(line));

            return list;
        }
        catch
        {
            // consider to handle them
            Console.WriteLine($"[ERROR] Can't read the ScraperOutput file ({filePath})");
            return list;
        }
    }

    public static void Write(List<RankingUrl> rankingsUrls, string dataFolder)
    {
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