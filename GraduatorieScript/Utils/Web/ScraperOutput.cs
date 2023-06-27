using GraduatorieScript.Data;
using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Web;

public static class ScraperOutput
{
    private static string GetFilePath(string? docFolder)
    {
        return docFolder + "/" + Constants.OutputLinksFilename;
    }

    public static void Write(IEnumerable<RankingUrl> urls, string? dataFolder)
    {
        var filePath = GetFilePath(dataFolder);
        var links = GetSaved(dataFolder);
        links.AddRange(urls);

        var online = links
            .Select(url => url.Url)
            .Where(UrlUtils.CheckUrl)
            .Distinct()
            .ToList();

        online.Sort();

        var output = "";
        foreach (var link in online)
        {
            output += link;
            output += "\n";
        }

        Console.WriteLine($"[INFO] ScraperOutput writing to file {filePath}: {online.Count} links");
        File.WriteAllText(filePath, output);
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
}