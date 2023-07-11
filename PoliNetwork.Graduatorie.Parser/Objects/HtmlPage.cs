using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HtmlPage
{
    private readonly string? _htmlString;
    public readonly HtmlDocument? Html;
    public readonly RankingUrl? Url;

    public HtmlPage(string html, RankingUrl url)
    {
        var page = new HtmlDocument();
        page.LoadHtml(html);
        Html = page;
        _htmlString = html;
        Url = url;
    }

    public override string? ToString()
    {
        return _htmlString;
    }

    public static HtmlPage? FromUrl(RankingUrl url, string htmlFolder)
    {
        var saved = GetLocalHtml(url, htmlFolder);
        if (!string.IsNullOrEmpty(saved)) return new HtmlPage(saved, url);

        // no saved file, need to download
        var html = Scraper.Utils.Web.Scraper.Download(url.Url);
        if (html is null || string.IsNullOrEmpty(html))
            return null;

        return new HtmlPage(html, url);
    }

    private static string? GetLocalHtml(RankingUrl url, string htmlFolder)
    {
        try
        {
            var localPath = url.GetLocalPath(htmlFolder);
            if (!File.Exists(localPath)) return null;

            var html = File.ReadAllText(localPath);
            if (html.ToLower().Contains("politecnico"))
                return html;

            // saved html is wrong
            File.Delete(localPath);
            return null;
        }
        catch
        {
            return null;
        }
    }

    public bool SaveLocal(string htmlFolder, bool force = false)
    {
        var localPath = Url?.GetLocalPath(htmlFolder);
        try
        {
            if (File.Exists(localPath) && !force)
                return true;

            Console.WriteLine($"[DEBUG] Saving HtmlPage with localPath = {localPath}");
            if (localPath != null)
                File.WriteAllText(localPath, _htmlString);

            return true;
        }
        catch
        {
            Console.WriteLine($"[ERROR] Can't save HtmlPage with localPath = {localPath}");
            return false;
        }
    }
}
