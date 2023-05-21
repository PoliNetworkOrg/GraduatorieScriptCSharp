using GraduatorieScript.Objects;
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
    private readonly HtmlWeb _web = new();
    private string[] _newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "merito", "nuovi studenti"
    };

    /// <summary>
    ///   Taken an href from the a tag which could be either an internal link or an 
    ///   external one, this method returns the full url using the polimi domain
    /// </summary>
    /// <param name="href">The href from the html anchor tag taken from the news content.</param>
    /// <returns>The full url</returns>
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

    public HashSetExtended<string> FindRankingsLink(IEnumerable<string> newsLink)
    {
        var rankingsList = new HashSetExtended<string>();

        Parallel.Invoke(newsLink
            .Select(currentLink => (Action)(() => { FindSingleRankingLink(rankingsList, currentLink); })).ToArray());
        return rankingsList;
    }

    private void FindSingleRankingLink(HashSet<string> rankingsList, string currentLink)
    {
        var htmlDoc = _web.Load(currentLink);
        var links = htmlDoc.DocumentNode.GetElementsByTagName("a")
            .Select(element => UrlifyLocalHref(element.GetAttributeValue("href", string.Empty)))
            .Where(url => url.Contains(Data.Constants.RisultatiAmmissionePolimiIt))
            .ToList();

        lock (rankingsList)
        {
            foreach (var variable in links)
            {
                rankingsList.Add(variable);
            }
        }
    }

    public static Ranking? Download(string? url)
    {
        using var client = new HttpClient();
        var  response = client.GetAsync(url);
        response.Wait();
        var content = response.Result.Content;
        var result = content.ReadAsStringAsync().Result;

        var rankingsSet = new RankingsSet();
        rankingsSet.AddFileRead(result);
        var download = rankingsSet.Rankings.Count > 0 ? rankingsSet.Rankings.First() : null;
        return download;
    }
}
