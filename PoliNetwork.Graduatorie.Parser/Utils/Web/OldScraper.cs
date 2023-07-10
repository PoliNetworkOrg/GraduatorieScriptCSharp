using GraduatorieCommon.Extensions;
using GraduatorieCommon.Objects;
using GraduatorieScript.Data;
using HtmlAgilityPack;
using PoliNetwork.Graduatorie.Common.Extensions;

namespace GraduatorieScript.Utils.Web;

public class OldScraper
{
    private const string TargetUrl = "http://www.risultati-ammissione.polimi.it";

    private static readonly List<string> HttpsPolimiIt = new() { "https://www.polimi.it", "https://polimi.it" };

    private static readonly HashSet<string> Navigated = new();

    private readonly List<string> _newsUrls = new()
    {
        "https://www.polimi.it",
        "https://www.polimi.it/futuri-studenti"
    };

    private readonly HtmlWeb _web = new();

    private string[] _newsTesters =
    {
        "graduatorie", "graduatoria", "punteggi", "tol",
        "immatricolazioni", "immatricolazione", "punteggio",
        "matricola", "merito", "nuovi studenti"
    };

    public IEnumerable<string?> GetNewsLinks()
    {
        var result = new WrapperList<string?>();
        var actions = new List<Action>();
        foreach (var newsUrl in _newsUrls)
        {
            var localResult = result;

            void Action()
            {
                var result2 = GetNewsLinks2(newsUrl);
                var links = result2.Where(value => !string.IsNullOrEmpty(value));
                foreach (var link in links)
                    lock (localResult)
                    {
                        localResult.Add(link);
                    }
            }

            actions.Add(Action);
        }

        ParallelRun.Run(actions.ToArray());


        var newsLinks = result.Distinct();
        Console.WriteLine(newsLinks.Count);
        return newsLinks;
    }

    private IEnumerable<string?> GetNewsLinks2(string variable)
    {
        var result = new List<string?>();
        var htmlDoc = _web.Load(variable);

        GetNewsLinks4(result, htmlDoc, variable, 0);
        GetNewsLinks5(result, htmlDoc);
        return result;
    }

    private void GetNewsLinks4(List<string?> result, HtmlDocument htmlDoc, string startWebsite, int depth)
    {
        var htmlNodeCollection = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

        var list = htmlNodeCollection.ToList();

        Action Selector(HtmlNode htmlNode)
        {
            return () =>
            {
                try
                {
                    var x = GetNewsLinks6(htmlNode, startWebsite, depth);
                    if (x == null) return;
                    foreach (var variable in x.Where(variable => !string.IsNullOrEmpty(variable)))
                        lock (result)
                        {
                            result.Add(variable);
                        }
                }
                catch
                {
                    // ignored
                }
            };
        }

        var action = list.Select((Func<HtmlNode, Action>)Selector).ToArray();
        InvokeSplit(action);
    }

    private static void InvokeSplit(IEnumerable<Action> action)
    {
        var list = SplitIntoChunks(action.ToList(), 10);
        var actionsEnumerable = list.Select(variable => variable.ToArray());
        foreach (var actions in actionsEnumerable) ParallelRun.Run(actions);
    }

    private static List<List<T>> SplitIntoChunks<T>(IReadOnlyCollection<T> list, int chunkSize)
    {
        var chunks = new List<List<T>>();

        for (var i = 0; i < list.Count; i += chunkSize)
        {
            var chunk = list.Skip(i).Take(chunkSize).ToList();
            chunks.Add(chunk);
        }

        return chunks;
    }

    private List<string?>? GetNewsLinks6(HtmlNode? arg, string startWebsite, int depth)
    {
        if (arg == null)
            return null;

        var href = arg.Attributes.Contains("href") ? arg.Attributes["href"].Value : null;
        if (string.IsNullOrEmpty(href))
            return null;

        href = href.Trim();

        if (href.StartsWith("#"))
            return null;

        //if (href.Contains("dettaglio-news")) ;

        if (href.StartsWith(TargetUrl))
            return new List<string?> { href };

        const int depthMax = 3;
        if (depth >= depthMax)
            return null;

        href = UrlUtils.UrlifyLocalHref(href, HttpsPolimiIt.First());


        if (href == startWebsite || !href.StartsWith(startWebsite)) return null;

        if (Navigated.Contains(href))
            return null;

        var htmlDoc = _web.Load(href);
        Navigated.Add(href);

        var result = new List<string?>();
        GetNewsLinks4(result, htmlDoc, href, depth + 1);
        return result;
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

        var url = UrlUtils.UrlifyLocalHref(href, HttpsPolimiIt.First());
        return new AnchorElement { Name = element.InnerText, Url = url };
    }

    public IEnumerable<string>? FindRankingsLink(IEnumerable<string?>? newsLink)
    {
        if (newsLink == null)
            return null;

        var rankingsList = new HashSet<string>();
        var actions = newsLink
            .Select(currentLink => (Action)(() => { FindSingleRankingLink(rankingsList, currentLink); }))
            .ToArray();
        ParallelRun.Run(actions);

        return rankingsList;
    }

    private void FindSingleRankingLink(HashSet<string> rankingsList, string? currentLink)
    {
        if (string.IsNullOrEmpty(currentLink))
            return;

        if (currentLink.Contains(Constants.RisultatiAmmissionePolimiIt))
        {
            rankingsList.Add(currentLink);
            return;
        }

        var htmlDoc = _web.Load(currentLink);
        var links = htmlDoc.DocumentNode.GetElementsByTagName("a")
            .Select(element =>
                UrlUtils.UrlifyLocalHref(element.GetAttributeValue("href", string.Empty), HttpsPolimiIt.First()))
            .Where(url => url.Contains(Constants.RisultatiAmmissionePolimiIt))
            .ToList();

        lock (rankingsList)
        {
            rankingsList.AddRange(links);
        }
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
            var content = response.Result.Content;
            var result = content.ReadAsStringAsync().Result;

            return result;
        }
        catch
        {
            // ignored
        }

        return null;
    }
}