using HtmlAgilityPack;

namespace GraduatorieScript.Utils;

internal struct AnchorElement
{
    public string Name;
    public string Url;
}

public class Scraper
{
    private const string NewsUrl = "https://www.polimi.it/in-evidenza";
    private readonly HtmlWeb _web = new();

    private string[] _newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "merito", "nuovi studenti"
    };

    private static string UrlifyLocalHref(string href)
    {
        return href.StartsWith("/") ? "https://polimi.it" + href : href;
    }

    public IEnumerable<string> GetNewsLinks()
    {
        var htmlDoc = _web.Load(NewsUrl);

        var anchorElements = htmlDoc.DocumentNode
            .SelectNodes("//*[@id=\"c42275\"]/ul/li/h3/a")
            .Select(element =>
            {
                var href = element.Attributes["href"].Value;
                var url = UrlifyLocalHref(href);
                return new AnchorElement { Name = element.InnerText, Url = url };
            })
            .ToList();

        var filteredLinks = anchorElements
            /* .Where(anchor => NewsTesters.Contains(anchor.Name.ToLower())) */
            .Select(anchor => anchor.Url)
            .ToList();

        return filteredLinks;
    }

    public List<string> FindRankingsLink(IEnumerable<string> newsLink)
    {
        var rankingsList = new List<string>();

        Parallel.Invoke(newsLink
            .Select(currentLink => (Action)(() => { FindSingleRankingLink(rankingsList, currentLink); })).ToArray());
        return rankingsList.Distinct().ToList();
    }

    private void FindSingleRankingLink(List<string> rankingsList, string currentLink)
    {
        var htmlDoc = _web.Load(currentLink);
        var links = htmlDoc.DocumentNode.GetElementsByTagName("a")
            .Select(element => UrlifyLocalHref(element.GetAttributeValue("href", string.Empty)))
            .Where(url => url.Contains("risultati-ammissione.polimi.it"))
            .ToList();

        lock (rankingsList)
        {
            rankingsList.AddRange(links);
        }
    }
}