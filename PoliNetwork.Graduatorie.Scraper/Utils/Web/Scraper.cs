#region

using HtmlAgilityPack;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Extensions;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public class Scraper
{
    private const string TargetUrl = Constants.RisultatiAmmissionePolimiIt;
    private const string BaseUrl = "https://www.polimi.it";
    private const string AvvisiFuturiStudentiUrl = "https://www.polimi.it/futuri-studenti/avvisi";

    private readonly HashSet<string> _alreadyVisited = new();

    private readonly string[] _newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "nuovi studenti"
    };

    private readonly HtmlWeb _web = new();

    public IEnumerable<string> GetRankingsLinks()
    {
        // before there were multiple source to get links.
        // atm rankings are published exclusively 
        // on AvvisiFuturiStudentiUrl and on TG channel
        // note: here we are using the web page
        return ScrapeAvvisiFuturiStudenti();
    }

    private IEnumerable<string> ScrapeAvvisiFuturiStudenti()
    {
        HashSet<string> links = new();
        var page = _web.Load(AvvisiFuturiStudentiUrl).DocumentNode;

        var newsCards =
            page.SelectNodes("//div[contains(@class, 'news')]//div[contains(@class, 'row--card')]//div[contains(@class, 'card__content')]");
        if (newsCards == null) return links;

        foreach (var card in newsCards)
        {
            var title = card.Descendants("h5").First();
            var titleValid = title != null && IsValidText(title.InnerText);

            var body = card.Descendants("p").Where(el => el.ParentNode.HasClass("news-bodytext")).First();
            var bodyValid = body != null && IsValidText(body.InnerText);

            var aTag = card.Descendants("a").First();

            if (!titleValid && !bodyValid && aTag != null) continue;

            var href = GetHref(aTag);
            links.AddRange(UseHref(href));
        }

        return links;
    }

    private IEnumerable<string> UseHref(string? href)
    {
        HashSet<string> links = new();
        if (string.IsNullOrEmpty(href)) return links;

        if (href.Contains(TargetUrl))
        {
            links.Add(href);
        }
        else
        {
            var url = UrlUtils.UrlifyLocalHref(href, BaseUrl);
            links.AddRange(ParseNewsPage(url));
        }

        return links;
    }

    private IEnumerable<string> ParseNewsPage(string url)
    {
        HashSet<string> links = new();
        if (!_alreadyVisited.Add(url)) return links;

        var page = _web.Load(url).DocumentNode;

        var aTags = page.SelectNodes("//div[contains(@class, 'news-text-wrap')]//a[@href]");
        if (aTags == null) return links;

        foreach (var a in aTags)
        {
            var href = GetHref(a);
            if (href != null && href.Contains(TargetUrl)) links.Add(href);
        }


        return links;
    }

    private bool IsValidText(string text)
    {
        var lower = text.ToLower();
        return _newsTesters.Any(test => lower.Contains(test));
    }

    private static string? GetHref(HtmlNode? a)
    {
        return a?.GetAttributeValue("href", string.Empty).Replace("amp;", "");
    }


    public static string? Download(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync(url);
            response.Wait();

            if (!response.Result.IsSuccessStatusCode) return null;

            var content = response.Result.Content;
            var result = content.ReadAsStringAsync().Result;
            return result;
        }
        catch
        {
            return null;
        }
    }
}
