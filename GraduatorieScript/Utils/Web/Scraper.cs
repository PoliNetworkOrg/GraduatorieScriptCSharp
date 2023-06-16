using GraduatorieScript.Data;
using GraduatorieScript.Extensions;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Transformer;
using HtmlAgilityPack;

namespace GraduatorieScript.Utils.Web;

internal struct AnchorElement
{
    public string Name;
    public string Url;
}

public class Scraper
{
 private const string HttpsPolimiIt = "https://polimi.it";
    private readonly List<string> newsUrl = new(){ "https://polimi.it", "https://www.polimi.it/futuri-studenti"};
    private const string targetUrl = "http://www.risultati-ammissione.polimi.it";
    private readonly HtmlWeb web = new();

    private string[] newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "merito", "nuovi studenti"
    };

    public IEnumerable<string?> GetNewsLinks()
    {
        var result = new List<string?>();
        foreach (var variable in newsUrl)
        {
            GetNewsLinks2(variable, result);
        }
        return result;
    }

    private void GetNewsLinks2(string variable, List<string?> result)
    {
        var htmlDoc = web.Load(variable);

        GetNewsLinks4(result, htmlDoc, variable);
        GetNewsLinks5(result, htmlDoc);
        
    }

    private void GetNewsLinks4(List<string?> result, HtmlDocument htmlDoc, string startWebsite)
    {
        var htmlNodeCollection = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        
        List<string?>? links = htmlNodeCollection?
            .SelectMany(x => GetNewsLinks6(x, startWebsite) ?? new List<string?>())
            .ToList();
        
        if (links == null) return;
        foreach (var variable in links)
        {
            if (!string.IsNullOrEmpty(variable))
                result.Add(variable);
        }
    }

    private List<string?>? GetNewsLinks6(HtmlNode arg, string startWebsite)
    {
        var href = arg.Attributes.Contains("href") ? arg.Attributes["href"].Value : null;
        if (string.IsNullOrEmpty(href))
            return null;

        href = href.Trim();

        if (href.StartsWith("#"))
            return null;

        if (href.StartsWith(targetUrl))
            return new List<string?>(){href};
        
        ;
        if (href != startWebsite && href.StartsWith(startWebsite))
        {
            var htmlDoc = web.Load(href);
            List<string?> result = new List<string?>();
            GetNewsLinks4(result, htmlDoc, href);
            return result;
        }

        return null;
    }

    private static void GetNewsLinks5(List<string?> result, HtmlDocument htmlDoc)
    {
        var htmlNodeCollection = htmlDoc.DocumentNode
            .SelectNodes("//*[@id=\"c42275\"]/ul/li/h3/a");
        var anchorElements = htmlNodeCollection?
            .Select(GetNewsLinks3)
            .ToList();

        if (anchorElements == null) return;
        var filteredLinks = anchorElements
            /* .Where(anchor => NewsTesters.Contains(anchor.Name.ToLower())) */
            .Select(anchor => anchor.Url)
            .ToList();
        result.AddRange(filteredLinks);
    }

    private static AnchorElement GetNewsLinks3(HtmlNode element)
    {
        var href = element.Attributes["href"].Value;

        var url = UrlUtils.UrlifyLocalHref(href, HttpsPolimiIt);
        return new AnchorElement { Name = element.InnerText, Url = url };
    }

    public IEnumerable<string> FindRankingsLink(IEnumerable<string> newsLink)
    {
        var rankingsList = new HashSet<string>();

        Parallel.Invoke(newsLink
            .Select(currentLink => (Action)(() => { FindSingleRankingLink(rankingsList, currentLink); })).ToArray());
        return rankingsList;
    }

    private void FindSingleRankingLink(HashSet<string> rankingsList, string currentLink)
    {
        var htmlDoc = web.Load(currentLink);
        var links = htmlDoc.DocumentNode.GetElementsByTagName("a")
            .Select(element => UrlUtils.UrlifyLocalHref(element.GetAttributeValue("href", string.Empty), HttpsPolimiIt))
            .Where(url => url.Contains(Constants.RisultatiAmmissionePolimiIt))
            .ToList();

        lock (rankingsList)
        {
            rankingsList.AddRange(links);
        }
    }

    public static Ranking? Download(string url)
    {
        using var client = new HttpClient();
        var response = client.GetAsync(url);
        response.Wait();
        var content = response.Result.Content;
        var result = content.ReadAsStringAsync().Result;

        var ranking = Parser.ParseHtml(result, RankingUrl.From(url));
        return ranking;
    }
}