using GraduatorieScript.Enums;
using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Web;

public static class ScraperOutput
{
    private const string FilePath = "links.txt";

    public static void Write(List<RankingUrl?>? rankingsUrls, string docFolder)
    {
        var filePath = docFolder + "/" + FilePath;
        if (File.Exists(filePath)) AddFromFile(rankingsUrls);

        var s = "";
        var variableUrls = rankingsUrls?.Select(variable => variable?.Url)
            .Where(variableUrl => !string.IsNullOrEmpty(variableUrl)).ToList();
        if (variableUrls != null)
        {
            variableUrls.Sort();
            foreach (var variableUrl in variableUrls)
            {
                s += variableUrl;
                s += "\n";
            }
        }

        Console.WriteLine($"Writing links to folder {filePath} at {DateTime.Now}");
        File.WriteAllText(filePath, s);
    }

    private static void AddFromFile(ICollection<RankingUrl?>? rankingsUrls)
    {
        var dir = Directory.GetCurrentDirectory();
        try
        {
            var x = File.ReadAllLines(dir + "\\" + FilePath);
            foreach (var variable in x)
                if (!string.IsNullOrEmpty(variable))
                    AddToList(rankingsUrls, variable);
        }
        catch
        {
            // ignored
        }
    }

    private static void AddToList(ICollection<RankingUrl?>? rankingsUrls, string variable)
    {
        var isPresent = FindIfPresent(rankingsUrls, variable);
        if (isPresent) return;
        rankingsUrls?.Add(new RankingUrl { PageEnum = PageEnum.Index, Url = variable });
    }

    private static bool FindIfPresent(IEnumerable<RankingUrl?>? rankingsUrls, string variable)
    {
        return rankingsUrls != null && rankingsUrls.Any(rankingUrl => variable == rankingUrl?.Url);
    }
}