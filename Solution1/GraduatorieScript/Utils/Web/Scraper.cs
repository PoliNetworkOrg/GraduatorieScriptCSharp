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
    private const string NewsUrl = "https://www.polimi.it/in-evidenza";
    private const string HttpsPolimiIt = "https://polimi.it";
    private readonly HtmlWeb _web = new();

    private string[] _newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "merito", "nuovi studenti"
    };

    public IEnumerable<string> GetNewsLinks()
    {
        var htmlDoc = _web.Load(NewsUrl);

        var anchorElements = htmlDoc.DocumentNode
            .SelectNodes("//*[@id=\"c42275\"]/ul/li/h3/a")
            .Select(element =>
            {
                var href = element.Attributes["href"].Value;

                var url = UrlUtils.UrlifyLocalHref(href, HttpsPolimiIt);
                return new AnchorElement { Name = element.InnerText, Url = url };
            })
            .ToList();

        var filteredLinks = anchorElements
            /* .Where(anchor => NewsTesters.Contains(anchor.Name.ToLower())) */
            .Select(anchor => anchor.Url)
            .ToList();
        return filteredLinks;
    }

    public HashSet<string> FindRankingsLink(IEnumerable<string> newsLink)
    {
        var rankingsList = new HashSet<string>();

        Parallel.Invoke(newsLink
            .Select(currentLink => (Action)(() => { FindSingleRankingLink(rankingsList, currentLink); })).ToArray());
        return rankingsList;
    }

    private void FindSingleRankingLink(HashSet<string> rankingsList, string currentLink)
    {
        var htmlDoc = _web.Load(currentLink);
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
