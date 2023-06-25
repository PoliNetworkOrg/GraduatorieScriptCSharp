﻿using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Extensions;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GraduatorieScript.Utils.Transformer;



public static class Parser
{
    public static RankingsSet GetRankings(
        string htmlFolder,
        string jsonPath,
        IEnumerable<RankingUrl> urls
    )
    {
        var rankingsSet = ParseLocalJson(jsonPath) ?? new RankingsSet();
        var savedHtmls = ParseLocalHtmlFiles(htmlFolder);
        var newUrls = urls.Where(u => savedHtmls.All(s => s.Url.Url != u.Url));
        var newHtmls = newUrls
            .Select(HtmlPage.FromUrl)
            .Where(h => h != null)
            .Select(h => h!) // for some reasons it still infered as null
            .ToList();

        /* var allHtmls = savedHtmls.Concat(newHtmls).ToList(); */
        var allHtmls = newHtmls;

        foreach(var html in allHtmls) {
            Console.WriteLine($"{html.Url.Url} {html.Url.PageEnum}");
        }

        var indexes = allHtmls.Where(h => h.Url.PageEnum == PageEnum.Index).ToList();
        allHtmls.RemoveAll(h => h.Url.PageEnum == PageEnum.Index);

        foreach (var index in indexes)
        {
            GetRankingsSingle(index, rankingsSet, allHtmls);
        }

        return rankingsSet;
    }

    private static void GetRankingsSingle(HtmlPage index, RankingsSet rankingsSet, List<HtmlPage> allHtmls)
    {
        Console.WriteLine($"[DEBUG] parsing index {index.Url.Url}");
        var findIndex = rankingsSet.Rankings.FindIndex(r => r.Url?.Url == index.Url.Url);
        var b1 = findIndex >= 0;
       
        if (b1)
        {
            var b2 = rankingsSet.Rankings[findIndex];
            if (b2 is { byMerit: not null, byCourse: not null })
            {
                Console.WriteLine($"[DEBUG] skipping index {index.Url.Url}: already parsed");
                return;
            }
        }

        var doc = index.Html.DocumentNode;

        // get ranking info
        var intestazioni = doc.GetElementsByClassName("intestazione").ToList().Select(i =>
            i.Descendants("#text").ToList()[0].InnerText
        ).ToList();
        var schoolStr = intestazioni[2].Split("\n")[0].ToLower();
        var school = GetSchoolEnum(schoolStr);
        var urlUrl = index.Url.Url;
        if (school == SchoolEnum.Unknown)
        {
            Console.WriteLine(
                $"[ERROR] School '{schoolStr}' not recognized (index: {urlUrl}); skipped"
            );
            return;
        }

        int year = Convert.ToInt16(intestazioni[1].Split("Year ")[1].Split("/")[0]);
        var phase = intestazioni[3].Split("- ")[1];
        var notes = intestazioni[4];

        var aTags = doc.GetElementsByClassName("titolo")
            .SelectMany(a => a.GetElementsByTagName("a")) // links to subindex
            .Where(a => !a.InnerText.Contains("matricola"))
            .ToList(); // filter out id ranking

        var lastUrlIndex = urlUrl.LastIndexOf('/');
        var baseDomain = urlUrl[..lastUrlIndex] + "/";

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
            var subIndex = allHtmls.ToList().Find(h => h.Url.Url == url.Url) ?? HtmlPage.FromUrl(url);
            if (subIndex is not null)
                subIndexes.Add(subIndex);
        }

        allHtmls.RemoveAll(
            h =>
                h.Url.PageEnum is PageEnum.IndexByMerit or PageEnum.IndexByCourse
        );

        Table<MeritTableRow> meritTable = new();
        List<Table<CourseTableRow>> courseTables = new();

        foreach (var html in subIndexes)
        {
            var page = html.Html.DocumentNode;
            var url = html.Url;
            var tablesLinks = page.SelectNodes("//td/a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(href => href != null)
                .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
                .Select(RankingUrl.From)
                .AsParallel()
                .ToList();

            List<HtmlPage> tablePages = new();

            Action Selector(RankingUrl urlSingle)
            {
                return () =>
                {
                    bool Predicate(HtmlPage h)
                    {
                        return h.Url.Url == urlSingle.Url;
                    }

                    var htmlPage = allHtmls.ToList().Find(Predicate) ?? HtmlPage.FromUrl(urlSingle);
                    if (htmlPage != null) tablePages.Add(htmlPage);
                };
            }

            Parallel.Invoke(tablesLinks.Select((Func<RankingUrl, Action>)Selector).ToArray());
            var urlPageEnum = url.PageEnum;
            switch (urlPageEnum)
            {
                case PageEnum.IndexByMerit:
                {
                    var table = JoinTables(tablePages);
                    meritTable =
                        Table<MeritTableRow>.Create(table.Headers, table.Sections, ParseMeritTable(table), null, null);
                    break;
                }
                case PageEnum.IndexByCourse:
                {
                    var tables = GetTables(tablePages);
                    foreach (var table in tables)
                    {
                        var courseTable = Table<CourseTableRow>.Create(table.Headers, table.Sections,
                            ParseCourseTable(table), table.CourseTitle, table.CourseLocation);
                        courseTables.Add(courseTable);
                    }

                    break;
                }
                default:
                    Console.WriteLine(
                        $"[ERROR] Unhandled sub index (url: {url.Url}, type: {html.Url.PageEnum})"
                    );
                    break;
            }
        }

        var ranking = new Ranking
        {
            year = year,
            phase = phase,
            extra = notes,
            Url = index.Url,
            school = school,
            LastUpdate = DateTime.Now,
            byCourse = new List<CourseTable>(),
        };

        if (meritTable.Data[0].id is not null && courseTables[0].Data[0].id is not null)
        {
            foreach (var course in courseTables)
            {
                var courseStudents = new List<StudentResult>();
                foreach (var row in course.Data)
                {
                    var absolute = meritTable.Data.Find(r => r.id == row.id);
                    var student = new StudentResult
                    {
                        id = row.id,
                        ofa = row.ofa,
                        result = row.result,
                        birthDate = row.birthDate,
                        canEnroll = row.canEnroll,
                        canEnrollInto = row.canEnroll ? absolute?.canEnrollInto : null,
                        positionAbsolute = absolute?.position,
                        positionCourse = row.position,
                        sectionsResults = row.sectionsResults,
                        englishCorrectAnswers = row.englishCorrectAnswers,
                    };
                    courseStudents.Add(student);
                }

                ranking.byCourse.Add(new CourseTable
                {
                    Title = course.CourseTitle,
                    Location = course.CourseLocation,
                    Sections = course.Sections,
                    Headers = course.Headers,
                    Rows = courseStudents.OrderBy(s => s.positionCourse).ToList()
                });
            }

            ranking.byMerit = new MeritTable
            {
                Headers = meritTable.Headers,
                Rows = meritTable.Data.Select(row =>
                {
                    var findInCourse = ranking.byCourse
                        .Select(course => course.Rows?.Find(r => r.id == row.id))
                        .Where(rowSingle => rowSingle is not null)
                        .ToList();

                    var withEnroll = findInCourse.Count > 0 ? findInCourse.Find(c => c!.canEnroll): null;
                    var withMaxPoints = findInCourse.Count > 0 ? findInCourse.OrderBy(c => c!.positionCourse).First() : null;
                    var courseData = withEnroll ?? withMaxPoints;

                    return new StudentResult
                    {
                        canEnroll = row.canEnroll,
                        canEnrollInto = row.canEnroll ? row.canEnrollInto : null,
                        id = row.id,
                        positionAbsolute = row.position,
                        result = row.result,
                        ofa = row.ofa,
                        positionCourse = courseData?.positionCourse,
                        englishCorrectAnswers = courseData?.englishCorrectAnswers,
                        sectionsResults = courseData?.sectionsResults,
                        birthDate = courseData?.birthDate,
                    };
                }).OrderBy(s => s.positionAbsolute).ToList()
            };
        }
        else
        {
            foreach (var course in courseTables)
            {
                var courseStudents = new List<StudentResult>();
                foreach (var row in course.Data)
                {
                    var student = new StudentResult
                    {
                        id = row.id,
                        ofa = row.ofa,
                        result = row.result,
                        birthDate = row.birthDate,
                        canEnroll = row.canEnroll,
                        canEnrollInto = row.canEnroll ? course.CourseTitle : null,
                        positionAbsolute = null,
                        positionCourse = row.position,
                        sectionsResults = row.sectionsResults,
                        englishCorrectAnswers = row.englishCorrectAnswers,
                    };
                    courseStudents.Add(student);
                }

                ranking.byCourse.Add(new CourseTable
                {
                    Title = course.CourseTitle,
                    Location = course.CourseLocation,
                    Sections = course.Sections,
                    Headers = course.Headers,
                    Rows = courseStudents.OrderBy(s => s.positionCourse).ToList()
                });
            }

            ranking.byMerit = new MeritTable
            {
                Headers = meritTable.Headers,
                Rows = meritTable.Data.Select(row => new StudentResult
                {
                    canEnroll = row.canEnroll,
                    canEnrollInto = row.canEnroll ? row.canEnrollInto : null,
                    id = row.id,
                    positionAbsolute = row.position,
                    result = row.result,
                    ofa = row.ofa,
                    positionCourse = null,
                    englishCorrectAnswers = null,
                    sectionsResults = null,
                    birthDate = null,
                }).OrderBy(s => s.positionAbsolute).ToList()
            };
        }

        Console.WriteLine($"[DEBUG] adding ranking {index.Url.Url}");
        AddRankingAndMerge(rankingsSet, ranking);
    }

    private static void AddRankingAndMerge(RankingsSet rankingsSet, Ranking ranking)
    {
        bool Predicate(Ranking x) => x.Url?.Url == ranking.Url?.Url;
        var isPresent = rankingsSet.Rankings.Any(Predicate);
        if (!isPresent)
        {
            rankingsSet.AddRanking(ranking);
            return;
        }

        var r = rankingsSet.Rankings.FirstOrDefault((Func<Ranking, bool>)Predicate);
        if (r == null)
        {
            rankingsSet.AddRanking(ranking);
            return;
        }

        r.Merge(ranking);
    }

    private static IEnumerable<Table<List<string>>> GetTables(IEnumerable<HtmlPage> pages)
    {
        var tables = pages
            .Select(page =>
            {
                var isCourse = page.Url.PageEnum == PageEnum.TableByCourse;
                var doc = page.Html.DocumentNode;
                var header = GetTableHeader(doc);
                var rows = doc.SelectNodes("//table[contains(@class, 'TableDati')]/tbody/tr")
                    .ToList();
                var fullTitle = isCourse ? doc.GetElementsByClassName("titolo").ToList()[0].InnerText : null;
                var title = isCourse ? fullTitle?.Split(" (")[0] : null;
                var location = isCourse ? GetLocation(fullTitle) : null;
                var rowsData = rows.Select(
                        row =>
                            row.Descendants("td")
                                .Select(node => node.InnerText)
                                .Where(text => !string.IsNullOrEmpty(text))
                                .ToList()
                    )
                    .ToList();
                return Table.Create(header.Item1, header.Item2, rowsData, title, location);
            })
            .ToList();

        return tables;
    }

    private static string? GetLocation(string? fullTitle)
    {
        var strings = fullTitle?.Split("(");
        var s = strings?[1];
        var split = s?.Split(")");
        return split?[0];
    }

    private static (List<string>, List<string>?) GetTableHeader(HtmlNode doc)
    {
        var rows = doc.SelectNodes("//table[contains(@class, 'TableDati')]/thead/tr");
        var badIndex = rows[0].Descendants("th").ToList().FindIndex(node => node.GetAttributeValue("colSpan", 1) > 1);
        var rowsText = rows.Select(row => row.Descendants("th").Select(th => th.Descendants("#text").ToList()[0].InnerText).ToList()).ToList();
        if (rows.Count == 1 || badIndex == -1) return (rowsText[0], null);

        // course table, need to build correct headers
        var headers = rowsText[0];
        var sections = rowsText[1];
        var goodHeaders = new List<string>();
        goodHeaders.AddRange(headers.GetRange(0, badIndex));
        goodHeaders.AddRange(sections);
        goodHeaders.AddRange(headers.GetRange(badIndex + 1, headers.Count - (badIndex + 1)));
        return (goodHeaders, sections);
    }

    private static Table<List<string>> JoinTables(IEnumerable<HtmlPage> pages)
    {
        var tables = GetTables(pages).ToList();
        var headers = tables[0].Headers;
        var sections = tables[0].Sections;
        var data = tables.SelectMany(table => table.Data).ToList();
        return Table.Create(headers, sections, data, null, null);
    }

    private static List<MeritTableRow> ParseMeritTable(Table<List<string>> table)
    {
        List<MeritTableRow> parsedRows = new();
        var headers = table.Headers.Select(h => h.ToLower()).ToList();

        var idIndex = headers.FindIndex(t => t.Contains("matricola"));
        var votoTestIndex = headers.FindIndex(t => t.Contains("voto"));
        var posIndex = headers.FindIndex(t => t.Contains("posizione"));
        var corsoIndex = headers.FindIndex(t => t.Contains("corso"));
        var ofaEngIndex = headers.FindIndex(t => t.Contains("ofa inglese"));
        var ofaTestIndex = headers.FindIndex(t => t.Contains("ofa test"));

        foreach (var row in table.Data)
        {
            var id = Table.GetFieldByIndex(row, idIndex);
            var votoTest = Table.GetFieldByIndex(row, votoTestIndex) ?? "0";
            var enrollCourse = Table.GetFieldByIndex(row, corsoIndex) ?? "";
            var position = Table.GetFieldByIndex(row, posIndex) ?? "-1";
            var enrollAllowed = !enrollCourse
                .ToLower()
                .Contains("immatricolazione non consentita");

            var ofa = new Dictionary<string, bool>();

            var ofaEng = Table.GetFieldByIndex(row, ofaEngIndex);
            if (ofaEng is not null) ofa.Add("ENG", ofaEng.ToLower().Contains("si"));

            var ofaTest = Table.GetFieldByIndex(row, ofaTestIndex);
            if (ofaTest is not null) ofa.Add("TEST", ofaTest.ToLower().Contains("si"));

            var parsedRow = new MeritTableRow
            {
                id = id,
                position = Convert.ToInt16(position),
                result = Convert.ToDecimal(votoTest.Replace(",", ".")),
                ofa = ofa,
                canEnrollInto = enrollAllowed ? enrollCourse : null,
                canEnroll = enrollAllowed
            };
            parsedRows.Add(parsedRow);
        }

        return parsedRows;
    }

    private static List<CourseTableRow> ParseCourseTable(Table<List<string>> table)
    {
        List<CourseTableRow> parsedRows = new();
        var headers = table.Headers.Select(h => h.ToLower()).ToList();
        foreach(var h in headers) Console.Write($"{h};");
        Console.WriteLine();

        var posIndex = headers.FindIndex(t => t.Contains("posizione"));
        var idIndex = headers.FindIndex(t => t.Contains("matricola"));
        var birthDateIndex = headers.FindIndex(t => t.Contains("nascita"));
        var enrollAllowedIndex = headers.FindIndex(t => t.Contains("consentita"));
        var votoTestIndex = headers.FindIndex(t => t.Contains("voto"));
        var ofaEngIndex = headers.FindIndex(t => t.Contains("ofa inglese"));
        var ofaTestIndex = headers.FindIndex(t => t.Contains("ofa test"));
        var englishCorrectAnswersIndex = headers.FindIndex(t => t.Contains("risposte esatte inglese"));

        var sectionsIndex = table.GetSectionsIndex();

        foreach (var row in table.Data)
        {
            ParseRow(row, idIndex, votoTestIndex, posIndex, birthDateIndex, enrollAllowedIndex, englishCorrectAnswersIndex, ofaEngIndex, ofaTestIndex, sectionsIndex, parsedRows);
        }
        return parsedRows;
    }

    private static void ParseRow(List<string> row, int idIndex, int votoTestIndex, int posIndex, int birthDateIndex,
        int enrollAllowedIndex, int englishCorrectAnswersIndex, int ofaEngIndex, int ofaTestIndex,
        Dictionary<string, int>? sectionsIndex, List<CourseTableRow> parsedRows)
    {
        var id = Table.GetFieldByIndex(row, idIndex);
        var votoTest = Convert.ToDecimal(Table.GetFieldByIndex(row, votoTestIndex)?.Replace(",", ".") ?? "0");
        var fieldByIndex = Table.GetFieldByIndex(row, posIndex) ?? "-1";
        var position = Convert.ToInt16(fieldByIndex);
        var birthDate = DateOnly.ParseExact(Table.GetFieldByIndex(row, birthDateIndex) ?? "", "dd/MM/yyyy");
        var enrollAllowed = Table.GetFieldByIndex(row, enrollAllowedIndex)?.ToLower().Contains("si") ?? false;
        var englishCorrectAnswers = Convert.ToInt16(Table.GetFieldByIndex(row, englishCorrectAnswersIndex) ?? "0");
        var ofa = new Dictionary<string, bool>();

        var ofaEng = Table.GetFieldByIndex(row, ofaEngIndex);
        if (ofaEng is not null) ofa.Add("ENG", ofaEng.ToLower().Contains("si"));

        var ofaTest = Table.GetFieldByIndex(row, ofaTestIndex);
        if (ofaTest is not null) ofa.Add("TEST", ofaTest.ToLower().Contains("si"));

        var sectionsResults = new Dictionary<string, decimal>();
        if (sectionsIndex is not null)
            foreach (var section in sectionsIndex)
            {
                sectionsResults.Add(section.Key, Convert.ToDecimal(row[section.Value].Replace(",", ".")));
            }

        var parsedRow = new CourseTableRow
        {
            id = id,
            position = position,
            result = votoTest,
            ofa = ofa,
            canEnroll = enrollAllowed,
            englishCorrectAnswers = englishCorrectAnswers,
            birthDate = birthDate,
            sectionsResults = sectionsResults
        };
        parsedRows.Add(parsedRow);
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
        HashSet<HtmlPage> elements = new();
        if (string.IsNullOrEmpty(path))
            return elements;

        var files = Directory.GetFiles(path, "*.html", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileRelativePath = file.Split(path)[1];

            // ignore because this is the file built
            // by previous script which is useless for this one
            // (and it also breaks our logic)
            if (fileRelativePath.Contains("index.html"))
                continue;

            var html = File.ReadAllText(file);
            var url = $"http://{Constants.RisultatiAmmissionePolimiIt}{fileRelativePath}";
            // no need to check if url is online
            // because the html is already stored

            elements.Add(new HtmlPage(html, RankingUrl.From(url)));
        }

        return elements;
    }

    private static RankingsSet? ParseLocalJson(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            return null;

        var fileContent = File.ReadAllText(jsonPath);
        if (string.IsNullOrEmpty(fileContent))
            return null;

        var rankingsSet = JsonConvert.DeserializeObject<RankingsSet>(fileContent);
        return rankingsSet;
    }
}
