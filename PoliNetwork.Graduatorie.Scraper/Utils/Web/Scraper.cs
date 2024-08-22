#region

using System.Diagnostics;
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

    private static readonly HttpClientHandler HttpClientHandler = new()
    {
        AllowAutoRedirect = false
    };

    private readonly HashSet<string> _alreadyVisited = new();

    private readonly HttpClient _httpClient = new(HttpClientHandler);

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
        ScrapeManifesti();
        return ScrapeAvvisiFuturiStudenti();
    }

    private IEnumerable<string> ScrapeAvvisiFuturiStudenti()
    {
        HashSet<string> links = new();
        var page = _web.Load(AvvisiFuturiStudentiUrl).DocumentNode;

        var newsCards =
            page.SelectNodes(
                "//div[contains(@class, 'news')]//div[contains(@class, 'row--card')]//div[contains(@class, 'card__content')]");
        if (newsCards == null) return links;

        foreach (var card in newsCards)
        {
            var title = card.Descendants("h5").First();
            var titleValid = title != null && IsValidText(title.InnerText);

            var body = card.Descendants("p").First(el => el.ParentNode.HasClass("news-bodytext"));
            var bodyValid = body != null && IsValidText(body.InnerText);

            var aTag = card.Descendants("a").First();

            if (!titleValid && !bodyValid && aTag != null) continue;

            var href = GetHref(aTag);
            links.AddRange(UseHref(href));
        }

        return links;
    }

    public SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, string>>> ScrapeManifesti()
    {
        var map = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, string>>>();

        const string designUrl = "https://polimi.it/formazione/corsi-di-laurea/dettaglio-corso/design-degli-interni";
        const string ingCivileUrl =
            "https://polimi.it/formazione/corsi-di-laurea/dettaglio-corso/ingegneria-per-lambiente-e-il-territorio";
        const string ingUrl = "https://polimi.it/formazione/corsi-di-laurea/dettaglio-corso/ingegneria-informatica";
        const string archUrbUrl =
            "https://polimi.it/formazione/corsi-di-laurea/dettaglio-corso/ingegneria-edile-architettura";

        string[] urls = { designUrl, ingCivileUrl, ingUrl, archUrbUrl };

        foreach (var url in urls)
        {
            var page = _web.Load(url).DocumentNode;
            var aTags = page.SelectNodes("//a/u[contains(text(),'Insegnamenti del piano di studi')]/..");
            if (aTags is not { Count: 1 }) continue;

            var aTag = aTags.First();
            if (aTag == null) continue;

            var link = GetHref(aTag);
            if (string.IsNullOrEmpty(link)) continue;

            var response = _httpClient.GetAsync(link).Result;
            var finalLink = response.Headers.Location;
            if (finalLink == null) continue;

            var manPage = _web.Load(finalLink).DocumentNode;

            var groups = manPage.SelectNodes("//*[@id='id_combocds']/tbody/tr[3]/td[2]/select/optgroup");
            foreach (var group in groups)
            {
                if (group == null) continue;

                var name = group.GetAttributeValue("label", string.Empty);
                if (string.IsNullOrEmpty(name)) continue;

                var cleanName = name.Split(" -").FirstOrDefault(name);

                if (!map.ContainsKey(cleanName))
                    map.Add(cleanName, new SortedDictionary<string, SortedDictionary<string, string>>());
                var groupMap = map[cleanName];
                if (groupMap == null) throw new UnreachableException();

                var options = group.ChildNodes;


                if (options == null) return map;

                foreach (var option in options)
                {
                    if (option == null) continue;

                    var value = option.GetAttributeValue("value", "0");
                    var courseName = option.InnerText.Split(" (").First();

                    var isNumber = int.TryParse(value, out var intValue);

                    if (!isNumber) continue;
                    if (intValue == 0) continue;

                    if (!groupMap.ContainsKey(courseName))
                        groupMap.Add(courseName, new SortedDictionary<string, string>());
                    var courseDict = groupMap[courseName];

                    var optionLink = new Uri(finalLink.AbsoluteUri).SetQueryVal("k_corso_la", intValue.ToString());
                    var newPage = _web.Load(optionLink).DocumentNode;

                    var courseLocationTd =
                        newPage.SelectNodes(
                            "//td[contains(@class, 'CenterBar')]/table[contains(@class, 'BoxInfoCard')]//tr[4]/td[4]");

                    string[] defaultLocation = { "DEFAULT" };
                    var courseLocations = courseLocationTd == null || courseLocationTd.Count == 0
                        ? defaultLocation
                        : courseLocationTd.First().InnerText.Replace("\t", "").Replace("\n", "").Split(",");

                    foreach (var courseLocation in courseLocations)
                    {
                        var cleanCourseLocation = courseLocation.Trim();
                        var manifestoLink = new Uri(optionLink.AbsoluteUri).RemoveQueryVal("__pj0")
                            .RemoveQueryVal("__pj1");
                        if (!courseDict.ContainsKey(cleanCourseLocation))
                            courseDict.Add(cleanCourseLocation, manifestoLink.AbsoluteUri);
                    }
                }
            }
        }

        return map;
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