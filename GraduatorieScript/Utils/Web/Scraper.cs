using GraduatorieScript.Data;
using GraduatorieScript.Extensions;
using HtmlAgilityPack;

namespace GraduatorieScript.Utils.Web;

public class Scraper
{
    private const string TargetUrl = Constants.RisultatiAmmissionePolimiIt;
    private const string HomepageUrl = "https://www.polimi.it";
    private const string FuturiStudentiUrl = "https://www.polimi.it/futuri-studenti";
    private const string InEvidenzaUrl = "https://www.polimi.it/in-evidenza";

    private readonly HashSet<string> alreadyVisited = new();

    private readonly string[] newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "nuovi studenti"
    };

    private readonly HtmlWeb web = new();

    public IEnumerable<string> GetRankingsLinks()
    {
        HashSet<string> links = new();

        var l1 = ScrapeHomepage();
        var l2 = ScrapeFuturiStudenti();
        var l3 = ScrapeInEvidenza();

        links.AddRange(l1, l2, l3);
        return links;
    }

    private IEnumerable<string> ScrapeHomepage()
    {
        HashSet<string> links = new();
        var page = web.Load(HomepageUrl).DocumentNode;
        var slides = page.SelectNodes("//section[@id='copertina']//div[contains(@class, 'sp-slides')]/div");
        foreach (var slide in slides)
        {
            var h1 = slide.Descendants("h1");
            if (h1 == null) continue;
            var text = h1.First().InnerText;
            if (!IsValidText(text)) continue;
            var a = slide.Descendants("a");
            var href = GetHref(a.First());
            links.AddRange(UseHref(href));
        }

        return links;
    }

    private IEnumerable<string> ScrapeFuturiStudenti()
    {
        HashSet<string> links = new();
        var page = web.Load(FuturiStudentiUrl).DocumentNode;
        var slides =
            page.SelectNodes("//section[@id='newsNoThumb' or @id='news']//div[contains(@class, 'sp-slides')]/div");
        foreach (var slide in slides)
        {
            var h1 = slide.Descendants("h1");
            var h1Valid = h1 != null && IsValidText(h1.First().InnerText);

            var p = slide.Descendants("p");
            var pValid = p != null && IsValidText(p.First().InnerText);


            if (!h1Valid && !pValid) continue;
            var aTags = slide.Descendants("a");
            foreach (var a in aTags)
            {
                var href = GetHref(a);
                links.AddRange(UseHref(href));
            }
        }

        return links;
    }

    private IEnumerable<string> ScrapeInEvidenza()
    {
        HashSet<string> links = new();
        var page = web.Load(InEvidenzaUrl).DocumentNode;
        var liTags = page.SelectNodes("//div[@id='content']//li");
        foreach (var li in liTags)
        {
            var h3 = li.GetElementsByTagName("h3");

            var a = h3.First().ChildNodes[0];
            var aValid = a != null && IsValidText(a.InnerText);

            var p = li.Descendants("p");
            var pValid = p != null && IsValidText(p.First().InnerText);

            if (!aValid && !pValid) continue;

            var href = GetHref(a);
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
            var url = UrlUtils.UrlifyLocalHref(href, HomepageUrl);
            links.AddRange(ParseNewsPage(url));
        }

        return links;
    }

    private IEnumerable<string> ParseNewsPage(string url)
    {
        HashSet<string> links = new();
        if (alreadyVisited.Contains(url)) return links;
        alreadyVisited.Add(url);

        var page = web.Load(url).DocumentNode;

        var aTags = page.SelectNodes("//div[@id='content']//a[@href]");
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
        return newsTesters.Any(test => lower.Contains(test));
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
