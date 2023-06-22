using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Extensions;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GraduatorieScript.Utils.Transformer;

public class HtmlPage
{
    public readonly HtmlDocument Html;
    public readonly RankingUrl? Url;

    public HtmlPage(string html, RankingUrl? url)
    {
        var page = new HtmlDocument();
        page.LoadHtml(html);
        Html = page;
        Url = url;
    }


    private HtmlPage(Tuple<Ranking?, HttpContent> html)
    {
        var result = html.Item2.ReadAsStringAsync().Result;
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(result);
        Html = htmlDocument;
    }

    public static HtmlPage? FromUrl(RankingUrl? url)
    {
        var html = Scraper.Download(url?.Url);
        if (html == null || string.IsNullOrEmpty(html.ToString()))
            return null;
        if (html.Item1 != null)
            html.Item1.Url = url;
        return new HtmlPage(html);
    }
}

public static class Parser
{
    public static RankingsSet GetRankings(
        string htmlFolder,
        string jsonPath,
        IEnumerable<RankingUrl?> urls
    )
    {
        var rankingsSet = new RankingsSet { LastUpdate = DateTime.Now };
        var savedHtmls = ParseLocalHtmlFiles(htmlFolder);
        var newUrls = urls.Where(u => savedHtmls.All(s => s.Url?.Url != u?.Url));
        var newHtmls = newUrls
            .Where(url => url != null)
            .Select(HtmlPage.FromUrl)
            .Where(h => h != null)
            .Select(h => h!) // for some reasons it still infered as null
            .ToList();

        var web = new HtmlWeb();
        var indexes = newHtmls.Where(h => h.Url?.PageEnum == PageEnum.Index).ToList();
        newHtmls.RemoveAll(h => h.Url?.PageEnum == PageEnum.Index);

        foreach (var index in indexes)
        {
            var doc = index.Html.DocumentNode;

            // get ranking info
            var intestazioni = doc.GetElementsByClassName("intestazione").ToList();
            var schoolStr = intestazioni[2].InnerText.Split("\n")[0].ToLower();
            var school = GetSchoolEnum(schoolStr);
            var urlUrl = index.Url?.Url;
            if (school == SchoolEnum.Unknown)
            {
                Console.WriteLine(
                    $"[ERROR] School '{schoolStr}' not recognized (index: {urlUrl}); skipped"
                );
                continue;
            }

            int year = Convert.ToInt16(intestazioni[1].InnerText.Split("Year ")[1].Split("/")[0]);
            var phase = intestazioni[3].InnerText.Split("\n")[0].Split("- ")[1];
            var notes = intestazioni[4].InnerText.Split("\n")[0];

            var aTags = doc.GetElementsByClassName("titolo")
                .SelectMany(a => a.GetElementsByTagName("a")) // links to subindex
                .Where(a => !a.InnerText.Contains("matricola"))
                .ToList(); // filter out id ranking

            var lastUrlIndex = urlUrl?.LastIndexOf('/') ?? -1;
            var baseDomain = urlUrl?[..lastUrlIndex] + "/";

            var subUrls = aTags
                .Select(a => a.GetAttributeValue("href", null))
                .Where(href => href != null)
                .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
                .Select(RankingUrl.From)
                .Where(
                    url =>
                        url.PageEnum is PageEnum.IndexByMerit or PageEnum.IndexByCourse
                )
                .ToList();

            List<HtmlPage> subIndexes = new();
            foreach (var url in subUrls)
            {
                var subIndex = newHtmls.ToList().Find(h => h.Url?.Url == url.Url);
                if (subIndex == null)
                {
                    var html = HtmlPage.FromUrl(url);
                    if (html != null) subIndex = html;
                }

                if (subIndex != null)
                    subIndexes.Add(subIndex);
            }

            newHtmls.RemoveAll(
                h =>
                    h.Url?.PageEnum is PageEnum.IndexByMerit or PageEnum.IndexByCourse
            );

            List<MeritTableRow> meritTable = new();
            List<List<CourseTableRow>> courseTables = new();

            foreach (var html in subIndexes)
            {
                var page = html.Html.DocumentNode;
                var tablesLinks = page.SelectNodes("//td/a")
                    .Select(a => a.GetAttributeValue("href", null))
                    .Where(href => href != null)
                    .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
                    .Select(RankingUrl.From)
                    .AsParallel()
                    .ToList();

                List<HtmlPage> tablePages = new();

                Action Selector(RankingUrl url)
                {
                    return () =>
                    {
                        bool Predicate(HtmlPage h)
                        {
                            return h?.Url?.Url == url?.Url;
                        }

                        var htmlPage = newHtmls.ToList().Find(Predicate) ?? HtmlPage.FromUrl(url);
                        if (htmlPage != null) tablePages.Add(htmlPage);
                    };
                }

                Parallel.Invoke(tablesLinks.Select((Func<RankingUrl, Action>)Selector).ToArray());
                switch (html.Url?.PageEnum)
                {
                    case PageEnum.IndexByMerit:
                    {
                        var table = JoinTables(tablePages);
                        meritTable = ParseMeritTable(school, table);
                        break;
                    }
                    case PageEnum.IndexByCourse:
                    {
                        var tables = GetTables(tablePages);
                        courseTables.AddRange(tables.Select(table => ParseCourseTable(school, table)));
                        break;
                    }
                    default:
                        Console.WriteLine(
                            $"[ERROR] Unhandled sub index (url: {html.Url?.Url}, type: {html.Url?.PageEnum})"
                        );
                        break;
                }
            }

            Console.WriteLine($"{meritTable.Count} {courseTables.Count}");
            // TODO: join rows into rankings
        }

        return rankingsSet;
    }

    private static IEnumerable<List<List<string>>> GetTables(IEnumerable<HtmlPage> pages)
    {
        var table = pages
            .Select(page =>
            {
                var doc = page.Html.DocumentNode;
                var header =
                    doc.SelectNodes("//table[contains(@class, 'TableDati')]/thead/tr/td/"); // da aggiustare e usare
                var rows = doc.SelectNodes("//table[contains(@class, 'TableDati')]/tbody/tr")
                    .ToList();
                var rowsData = rows.Select(
                        row =>
                            row.Descendants("td")
                                .Select(node => node.InnerText)
                                .Where(text => !string.IsNullOrEmpty(text))
                                .ToList()
                    )
                    .ToList();
                return rowsData;
            })
            .ToList();

        return table;
    }

    private static List<List<string>> JoinTables(List<HtmlPage> pages)
    {
        var table = GetTables(pages).SelectMany(list => list).ToList();
        return table;
    }

    private static List<MeritTableRow> ParseMeritTable(SchoolEnum school, List<List<string>> table)
    {
        List<MeritTableRow> rows = new();
        foreach (var row in table)
        {
            var colsNum = row.Count;
            switch (school)
            {
                case SchoolEnum.Urbanistica:
                {
                    // no ofa
                    var hasId = colsNum == 4;
                    var o = hasId ? 1 : 0;

                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));
                    var posAbs = Convert.ToInt16(row[1 + o]);
                    var enrollCourse = row[2 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");
                    rows.Add(
                        new MeritTableRow
                        {
                            id = id,
                            result = votoTest,
                            position = posAbs,
                            canEnroll = enrollAllowed,
                            canEnrollInto = enrollAllowed ? enrollCourse : null
                        }
                    );
                    break;
                }
                case SchoolEnum.Architettura:
                case SchoolEnum.Design:
                {
                    // solo ofa inglese
                    var hasId = colsNum == 5;
                    var o = hasId ? 1 : 0;
                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));

                    var ofaDict = new Dictionary<string, bool>();
                    var hasOfaEng = row[1 + o].ToLower().Contains("si");
                    ofaDict["ENG"] = hasOfaEng;

                    var posAbs = Convert.ToInt16(row[2 + o]);
                    var enrollCourse = row[3 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");
                    var tRow = new MeritTableRow
                    {
                        id = id,
                        result = votoTest,
                        position = posAbs,
                        ofa = ofaDict,
                        canEnroll = enrollAllowed,
                        canEnrollInto = enrollAllowed ? enrollCourse : null
                    };
                    rows.Add(tRow);
                    break;
                }
                case SchoolEnum.Ingegneria:
                {
                    // has ofa test and ofa eng
                    var hasId = colsNum == 6;
                    var o = hasId ? 1 : 0;

                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));

                    var ofaDict = new Dictionary<string, bool>();
                    var hasOfaTest = row[1 + o].ToLower().Contains("si");
                    ofaDict["TEST"] = hasOfaTest;
                    var hasOfaEng = row[2 + o].ToLower().Contains("si");
                    ofaDict["ENG"] = hasOfaEng;

                    var posAbs = Convert.ToInt16(row[3 + o]);
                    var enrollCourse = row[4 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");
                    rows.Add(
                        new MeritTableRow
                        {
                            id = id,
                            result = votoTest,
                            position = posAbs,
                            ofa = ofaDict,
                            canEnroll = enrollAllowed,
                            canEnrollInto = enrollAllowed ? enrollCourse : null
                        }
                    );
                    break;
                }
            }
        }

        return rows;
    }

    private static List<CourseTableRow> ParseCourseTable(
        SchoolEnum school,
        List<List<string>> table
    )
    {
        List<CourseTableRow> rows = new();
        foreach (var row in table)
        {
            var colsNum = row.Count;
            switch (school)
            {
                case SchoolEnum.Urbanistica:
                {
                    // no ofa
                    var hasId = colsNum == 4;
                    var o = hasId ? 1 : 0;

                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));
                    var posAbs = Convert.ToInt16(row[1 + o]);
                    var enrollCourse = row[2 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");

                    //todo
                    /*
                rows.Add(
                );
                */
                    break;
                }
                case SchoolEnum.Architettura:
                case SchoolEnum.Design:
                {
                    // solo ofa inglese
                    var hasId = colsNum == 13;
                    var o = hasId ? 1 : 0;
                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));

                    var ofaDict = new Dictionary<string, bool>();
                    var hasOfaEng = row[1 + o].ToLower().Contains("si");
                    ofaDict["ENG"] = hasOfaEng;

                    var posAbs = Convert.ToInt16(row[2 + o]);
                    var enrollCourse = row[3 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");

                    //todo
                    //rows.Add(tRow);
                    break;
                }
                case SchoolEnum.Ingegneria:
                {
                    // has ofa test and ofa eng
                    var hasId = colsNum == 6;
                    var o = hasId ? 1 : 0;

                    var id = hasId ? row[0] : null;
                    var votoTest = Convert.ToDecimal(row[0 + o].Replace(",", "."));

                    var ofaDict = new Dictionary<string, bool>();
                    var hasOfaTest = row[1 + o].ToLower().Contains("si");
                    ofaDict["TEST"] = hasOfaTest;
                    var hasOfaEng = row[2 + o].ToLower().Contains("si");
                    ofaDict["ENG"] = hasOfaEng;

                    var posAbs = Convert.ToInt16(row[3 + o]);
                    var enrollCourse = row[4 + o];
                    var enrollAllowed = !enrollCourse
                        .ToLower()
                        .Contains("immatricolazione non consentita");


                    //todo
                    /*
                rows.Add(
                );
                */
                    break;
                }
            }
        }

        return rows;
    }

    private static SchoolEnum GetSchoolEnum(string schoolStr)
    {
        if (schoolStr.Contains("design"))
            return SchoolEnum.Design;
        if (schoolStr.Contains("ingegneria"))
            return SchoolEnum.Ingegneria;
        if (schoolStr.Contains("architettura"))
            return SchoolEnum.Architettura;
        if (schoolStr.Contains("urbanistica"))
            return SchoolEnum.Urbanistica;
        return SchoolEnum.Unknown;
    }

    private static HashSet<HtmlPage> ParseLocalHtmlFiles(string path)
    {
        var elements = new HashSet<HtmlPage>();
        if (string.IsNullOrEmpty(path))
            return elements;

        var files = Directory.GetFiles(path, "*.html", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileRelativePath = file.Split(path)[1];

            // ignore because this is the file built
            // by previous script which is useless for this one
            // (and it also breaks our logic)
            if (fileRelativePath == "index.html")
                continue;

            var html = File.ReadAllText(file);
            var url = $"http://{Constants.RisultatiAmmissionePolimiIt}{fileRelativePath}";
            // no need to check if url is online
            // because the html is already stored

            elements.Add(new HtmlPage(html, RankingUrl.From(url)));
        }

        return elements;
    }

    public static RankingsSet? FindParseHtmls(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        var files = Directory.GetFiles(path, "*.html", SearchOption.AllDirectories);
        var rankingsSet = new RankingsSet { LastUpdate = DateTime.Now };
        foreach (var file in files)
        {
            var fileRelativePath = file.Split(path)[1];

            // ignore because this is the file built
            // by previous script which is useless for this one
            // (and it also breaks our logic)
            if (fileRelativePath == "index.html")
                continue;

            var html = File.ReadAllText(file);
            var url = $"http://{Constants.RisultatiAmmissionePolimiIt}{fileRelativePath}";
            // no need to check if url is online
            // because the html is already stored


            var ranking = ParseHtml(html, RankingUrl.From(url));
            if (ranking != null)
                rankingsSet.AddRanking(ranking);
        }

        return rankingsSet;
    }

    public static Ranking? ParseHtml(string html, RankingUrl? url)
    {
        if (string.IsNullOrEmpty(html) || url?.PageEnum == PageEnum.Unknown)
            return null;

        //todo: da un testo formattato in html, ottenere la graduatoria o ogni altra informazione
        //e aggiungerla alla classe attuale, evitando ripetizioni

        var page = new HtmlDocument();
        page.LoadHtml(html);
        var doc = page.DocumentNode;

        var intestazione = doc.GetElementsByClassName("intestazione")
            .Select(el => el.InnerText)
            .First(text => text.Contains("Politecnico"));

        if (string.IsNullOrEmpty(intestazione))
            return null;

        Console.WriteLine($"{url?.Url} {url?.PageEnum} valid");
        
        
        //TODO: throw new NotImplementedException(); // just as a reminder
        
        return new Ranking(){Url = url};
 
    }

    public static RankingsSet? ParseLocalJson(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            return null;

        var fileContent = File.ReadAllText(jsonPath);
        if (string.IsNullOrEmpty(fileContent))
            return null;

        var rankingsSet = JsonConvert.DeserializeObject<RankingsSet>(fileContent);
        return rankingsSet;
    }

    public static RankingsSet ParseWeb(List<RankingUrl?>? rankingsUrls, string docFolder)
    {
        //download delle graduatorie, ricorsivamente, e inserimento nel rankingsSet
        var rankingsSet = new RankingsSet
        {
            LastUpdate = DateTime.Now,
            Rankings = new List<Ranking?>()
        };

        if (rankingsUrls == null) return rankingsSet;

        ScraperOutput.Write(rankingsUrls, docFolder);


        var select = rankingsUrls
            .Select(r => Scraper.Download(r?.Url));
        var enumerable = select
            .Where(download => download != null);
        
        foreach (var download in enumerable)
        {
            AddToList(rankingsSet, download);
        }

        return rankingsSet;
    }

    private static void AddToList(RankingsSet rankingsSet, Tuple<Ranking?, HttpContent>? download)
    {
        if (download?.Item1 == null)
            return;
        
        rankingsSet.Rankings.Add(download.Item1);
    }
}