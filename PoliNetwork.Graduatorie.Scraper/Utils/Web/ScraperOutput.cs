#region

using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public static class ScraperOutput
{
    public static List<RankingUrl> GetWithUrlsFromLocalFileLinks(IEnumerable<RankingUrl> urls, string dataFolder)
    {
        var links = GetSaved(dataFolder);
        links.AddRange(urls);
        return links.DistinctBy(r => r.Url).ToList();
    }

    private static List<RankingUrl> GetSaved(string dataFolder)
    {
        List<RankingUrl> list = new();
        var filePath = GetLinksFilePath(dataFolder);
        if (!File.Exists(filePath)) return list;

        var urls = GetLines(filePath);
        try
        {
            return urls.Select(RankingUrl.From).ToList();
        }
        catch
        {
            // consider to handle them
            Console.WriteLine($"[ERROR] Can't validate the ScraperOutput file ({filePath})");
            return new List<RankingUrl>();
        }
    }

    private static IEnumerable<string> GetLines(string filePath)
    {
        try
        {
            return File.ReadAllLines(filePath).Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            Console.WriteLine($"[ERROR] Can't read the ScraperOutput file ({filePath})");
            return new List<string>();
        }
    }

    public static void WriteLinks(List<RankingUrl> rankingsUrls, string? dataFolder)
    {
        if (string.IsNullOrEmpty(dataFolder))
            return;

        var filePath = GetLinksFilePath(dataFolder);

        var output = GetOutputLinksString(rankingsUrls);

        Console.WriteLine($"[INFO] ScraperOutput writing to file {filePath}: {rankingsUrls.Count} links");
        File.WriteAllText(filePath, output);
    }

    public static void WriteManifesti(
        SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, string>>> manifesti, string? dataFolder)
    {
        if (string.IsNullOrEmpty(dataFolder))
            return;

        var filePath = GetManifestiFilePath(dataFolder);

        var jsonString = JsonConvert.SerializeObject(manifesti, Culture.JsonSerializerSettings);

        var count = manifesti.Sum(a => a.Value.Sum(b => b.Value.Count));
        
        Console.WriteLine($"[INFO] ScraperOutput writing to file {filePath}: {count} manifesti");
        File.WriteAllText(filePath, jsonString);
        
    }

    private static string GetOutputLinksString(IEnumerable<RankingUrl> rankingsUrls)
    {
        var rankingLinksHashSet = CheckUrlUtil.GetRankingLinksHashSet(rankingsUrls);
        var rankingUrls = rankingLinksHashSet.Where(PredicateStringUrlNotNullNorEmpty);
        var urls = rankingUrls.Order();

        var enumerable1 = urls.Select(link => link.Url);
        var select = enumerable1.Select(SelectorUrlWithEndLine);
        var enumerable = select.Distinct().Order();
        return enumerable.Aggregate("", (current, linkUrl) => current + linkUrl);
    }

    private static bool PredicateStringUrlNotNullNorEmpty(RankingUrl x)
    {
        return !string.IsNullOrEmpty(x.Url);
    }

    private static string SelectorUrlWithEndLine(string url)
    {
        return url + "\n";
    }

    private static string GetLinksFilePath(string dataFolder)
    {
        return Path.Join(dataFolder, Constants.OutputLinksFilename);
    }

    private static string GetManifestiFilePath(string dataFolder)
    {
        
        return Path.Join(dataFolder, Constants.OutputFolder, Constants.OutputManifestiFilename);
    }
}