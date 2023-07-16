using HtmlAgilityPack;
using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Extensions;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Common.Utils.HashNS;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;
using PoliNetwork.Graduatorie.Scraper.Utils.Web;

/* using PoliNetwork.Graduatorie.Common.Utils.ParallelNS; */

namespace PoliNetwork.Graduatorie.Parser.Utils.Transformer.ParserNS;

public static class Parser
{
    public static RankingsSet? GetRankings(ArgsConfig argsConfig, IEnumerable<RankingUrl> urls)
    {
        if (string.IsNullOrEmpty(argsConfig.DataFolder))
            return null;

        var rankingsSet = BySchoolYearJson.Parse(argsConfig.DataFolder) ?? new RankingsSet();
        var restoredRankings = rankingsSet.Rankings.Count;
        if (restoredRankings > 0)
            Console.WriteLine($"[INFO] restored {restoredRankings} rankings");

        var htmlFolder = Path.Join(argsConfig.DataFolder, Constants.HtmlFolder);
        var savedHtmls = ParseLocalHtmlFiles(htmlFolder);

        var recursiveHtmls = urls.Where(url => url.PageEnum == PageEnum.Index)
            .Select(url => HtmlPage.FromUrl(url, htmlFolder))
            .Where(h => h is not null)
            .SelectMany(h => GetAndSaveAllHtmls(h!, htmlFolder));

        var allHtmls = savedHtmls.Concat(recursiveHtmls).ToList();

        var indexes = allHtmls.Where(h => h.Url?.PageEnum == PageEnum.Index).ToList();
        allHtmls.RemoveAll(h => h.Url?.PageEnum == PageEnum.Index);

        foreach (var index in indexes)
            GetRankingsSingle(index, rankingsSet, allHtmls, argsConfig.ForceReparsing ?? false);

        /* Action Selector(HtmlPage index) */
        /* { */
        /*     return () => */
        /*     { */
        /*     }; */
        /* } */
        /*  */
        /* var action = indexes.Select((Func<HtmlPage, Action>)Selector).ToArray(); */
        /* ParallelRun.Run(action); */

        rankingsSet.Rankings = rankingsSet.Rankings
            .OrderBy(x => x.School)
            .ThenBy(x => x.Year)
            .ThenBy(x => x.Url?.Url)
            .ToList();
        return rankingsSet;
    }

    private static IEnumerable<RankingUrl>? GetSubUrls(HtmlPage index)
    {
        var doc = index.Html?.DocumentNode;
        var aTags = doc?.GetElementsByClassName("titolo")
            .SelectMany(a => a.GetElementsByTagName("a")) // links to subindex
            .Where(a => !a.InnerText.Contains("matricola"))
            .ToList(); // filter out id ranking

        var baseDomain = index.Url?.GetBaseDomain();

        var subUrls = aTags
            ?.Select(a => a.GetAttributeValue("href", null))
            .Where(href => href != null)
            .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
            .Select(RankingUrl.From)
            .Where(url => url.PageEnum is PageEnum.IndexByMerit or PageEnum.IndexByCourse)
            .ToList();

        return subUrls;
    }

    private static IEnumerable<RankingUrl>? GetTableLinks(HtmlPage html)
    {
        var baseDomain = html.Url?.GetBaseDomain();

        var page = html.Html?.DocumentNode;
        var tablesLinks = page?.SelectNodes("//td/a")
            .ToList()
            .Select(a => a.GetAttributeValue("href", null))
            .Where(href => href != null)
            .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
            .Select(RankingUrl.From)
            .ToList();

        return tablesLinks;
    }

    private static IEnumerable<HtmlPage> GetAndSaveAllHtmls(HtmlPage index, string htmlFolder)
    {
        List<HtmlPage> newHtmls = new();
        index.SaveLocal(htmlFolder);
        newHtmls.Add(index);

        var subUrls = GetSubUrls(index);
        var subHtmls = subUrls?.Select(url => HtmlPage.FromUrl(url, htmlFolder)).ToList();
        if (subHtmls == null) return newHtmls;

        foreach (var subHtml in subHtmls) GetAndSaveAllHtmls2(htmlFolder, subHtml, newHtmls);

        /* var actions = subHtmls */
        /*     ?.Select( */
        /*         subHtml => */
        /*             (Action)( */
        /*                 () => */
        /*                 { */
        /*                 } */
        /*             ) */
        /*     ) */
        /*     .ToArray(); */
        /*  */
        /* if (actions != null) */
        /*     ParallelRun.Run(actions); */

        return newHtmls;
    }

    private static void GetAndSaveAllHtmls2(
        string htmlFolder,
        HtmlPage? subHtml,
        ICollection<HtmlPage> newHtmls
    )
    {
        if (subHtml is null)
            return;
        subHtml.SaveLocal(htmlFolder);
        newHtmls.Add(subHtml);

        var tableLinks = GetTableLinks(subHtml);
        var tableHtmls = tableLinks?.Select(url => HtmlPage.FromUrl(url, htmlFolder)).ToList();

        if (tableHtmls == null) return;
        foreach (var html in tableHtmls)
        {
            if (html == null) continue;
            html.SaveLocal(htmlFolder);
            newHtmls.Add(html);
        }

        /* var action = tableHtmls */
        /*     ?.Select( */
        /*         tableHtml => */
        /*             (Action)( */
        /*                 () => */
        /*                 { */
        /*                     if (tableHtml is null) */
        /*                         return; */
        /*                     tableHtml.SaveLocal(htmlFolder); */
        /*                     newHtmls.Add(tableHtml); */
        /*                 } */
        /*             ) */
        /*     ) */
        /*     .ToArray(); */
        /* if (action != null) */
        /*     ParallelRun.Run(action); */
    }

    private static void GetRankingsSingle(
        HtmlPage index,
        RankingsSet rankingsSet,
        ICollection<HtmlPage> allHtmls,
        bool forceReparsing
    )
    {
        Console.WriteLine($"[DEBUG] parsing index {index.Url?.Url}");
        if (rankingsSet.Rankings.Count > 0)
        {
            var findIndex = rankingsSet.Rankings.FindIndex(r => r.Url?.Url == index.Url?.Url);
            if (findIndex >= 0)
            {
                var parsed = rankingsSet.Rankings[findIndex];
                if (parsed is { ByMerit: not null, ByCourse: not null })
                    if (!forceReparsing)
                    {
                        Console.WriteLine(
                            $"[DEBUG] skipping index {index.Url?.Url}: already parsed"
                        );
                        return;
                    }
            }
        }

        var doc = index.Html?.DocumentNode;

        // get ranking info
        var intestazioni = doc?.GetElementsByClassName("intestazione")
            .ToList()
            .Select(i => i.Descendants("#text").ToList()[0].InnerText)
            .ToList();
        var schoolStr = intestazioni?[2].Split("\n")[0].ToLower();
        var school = GetSchoolEnum(schoolStr);
        var urlUrl = index.Url?.Url;
        if (school == SchoolEnum.Unknown)
        {
            Console.WriteLine(
                $"[ERROR] School '{schoolStr}' not recognized (index: {urlUrl}); skipped"
            );
            return;
        }

        int year = Convert.ToInt16(intestazioni?[1].Split("Year ")[1].Split("/")[0]);
        var strings = intestazioni?[3].Split(" - ")[1..];
        var enumerable = strings ?? Array.Empty<string>();
        var phase = string.Join(" ", enumerable);
        var notes = intestazioni?[4];

        var subUrls = GetSubUrls(index);

        List<HtmlPage> subIndexes = new();
        var subIndices = subUrls?.Select(url => SubIndex(allHtmls, url));
        if (subIndices != null)
            foreach (var subIndex in subIndices)
            {
                if (subIndex == null)
                    continue;
                subIndexes.Add(subIndex);
                allHtmls.Remove(subIndex);
            }

        Table<MeritTableRow> meritTable = new();
        List<Table<CourseTableRow>> courseTables = new();

        foreach (var html in subIndexes)
            GetRankingSingleSub(html, ref meritTable, courseTables, allHtmls);

        var ranking = new Ranking
        {
            Year = year,
            RankingOrder = new RankingOrder(phase),
            Extra = notes,
            Url = index.Url,
            School = school,
            LastUpdate = DateTime.Now,
            ByCourse = new List<CourseTable>()
        };

        foreach (var course in courseTables)
            ranking.ByCourse.Add(
                new CourseTable
                {
                    Title = course.CourseTitle,
                    Location = course.CourseLocation,
                    Sections = course.Sections,
                    Headers = course.Headers,
                    Rows = GetCourseStudents(course, meritTable),
                    Year = year,
                    Path = index.Url?.Url
                }
            );

        ranking.ByCourse = ranking.ByCourse.OrderBy(x => x.Title).ThenBy(x => x.Location).ToList();
        ranking.ByMerit = new MeritTable
        {
            Year = year,
            Path = index.Url?.Url,
            Headers = meritTable.Headers,
            Rows = GetMeritStudents(meritTable, ranking.ByCourse)
        };

        ranking.RankingSummary = ranking.CreateSummary();

        Console.WriteLine($"[DEBUG] adding ranking {index.Url?.Url}");

        AddRankingAndMerge(rankingsSet, ranking, forceReparsing);
    }

    private static List<StudentResult> GetMeritStudents(
        Table<MeritTableRow> table,
        IReadOnlyCollection<CourseTable> courses
    )
    {
        return table.Data
            .Select(row => MeritTableRowToStudentResult(row, courses))
            .OrderBy(s => s.PositionAbsolute)
            .ToList();
    }

    private static StudentResult MeritTableRowToStudentResult(
        MeritTableRow row,
        IEnumerable<CourseTable> courses
    )
    {
        var canEnroll = row.CanEnroll ?? false;
        var student = new StudentResult
        {
            CanEnroll = canEnroll,
            CanEnrollInto = canEnroll ? row.CanEnrollInto : null,
            Id = row.Id,
            PositionAbsolute = row.Position,
            Result = row.Result,
            Ofa = row.Ofa
        };

        if (row.Id == null)
            return student;

        var coursesRows = courses
            .Where(course => course.Rows is { Count: > 0 })
            .Select(course => course.Rows!)
            .ToList();

        if (coursesRows.Count == 0)
            return student;

        var studentCoursesRows = coursesRows
            .Select(rows => rows.Find(r => r.Id == row.Id))
            .Where(studentResult => studentResult is not null)
            .ToList();

        if (studentCoursesRows.Count == 0)
            return student;

        var finalRow = canEnroll
            ? studentCoursesRows.Find(c => c?.CanEnroll ?? false)
            : studentCoursesRows.OrderBy(c => c?.PositionCourse).First();

        if (finalRow == null)
            return student;

        student.PositionCourse = finalRow.PositionCourse;
        student.EnglishCorrectAnswers = finalRow.EnglishCorrectAnswers;
        student.SectionsResults = finalRow.SectionsResults;
        student.BirthDate = finalRow.BirthDate;
        return student;
    }

    private static List<StudentResult> GetCourseStudents(
        Table<CourseTableRow> course,
        Table<MeritTableRow> merit
    )
    {
        return course.Data
            .Select(row => CourseTableRowToStudentResult(merit.Data, row, course))
            .ToList()
            .OrderBy(s => s.PositionCourse)
            .ToList();
    }

    private static StudentResult CourseTableRowToStudentResult(
        List<MeritTableRow> meritTableData,
        CourseTableRow row,
        Table<CourseTableRow> course
    )
    {
        var canEnroll = row.CanEnroll ?? false;
        var student = new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = row.BirthDate,
            CanEnroll = canEnroll,
            CanEnrollInto = canEnroll ? course.CourseTitle : null,
            PositionCourse = row.Position,
            SectionsResults = row.SectionsResults,
            EnglishCorrectAnswers = row.EnglishCorrectAnswers
        };

        if (row.Id == null)
            return student;

        var meritRow = meritTableData.Find(r => r.Id == row.Id);
        if (meritRow == null)
            return student;

        student.PositionAbsolute = meritRow.Position;
        student.CanEnrollInto = canEnroll ? meritRow.CanEnrollInto : null;
        return student;
    }

    private static void GetRankingSingleSub(
        HtmlPage html,
        ref Table<MeritTableRow> meritTable,
        ICollection<Table<CourseTableRow>> courseTables,
        IEnumerable<HtmlPage> allHtmls
    )
    {
        var tableLinks = GetTableLinks(html);
        if (tableLinks == null) return;

        List<HtmlPage> tablePages = new();

        var htmlPages = allHtmls.ToList();
        foreach (var urlSingle in tableLinks)
        {
            var htmlPage = SubIndex(htmlPages, urlSingle);
            if (htmlPage != null)
                tablePages.Add(htmlPage);
        }

        /* Action Selector(RankingUrl urlSingle) */
        /* { */
        /*     return () => */
        /*     { */
        /*     }; */
        /* } */
        /*  */
        /* var actions = tableLinks?.Select((Func<RankingUrl, Action>)Selector).ToArray(); */
        /* if (actions != null) */
        /*     ParallelRun.Run(actions); */
        var urlPageEnum = html.Url?.PageEnum;
        switch (urlPageEnum)
        {
            case PageEnum.IndexByMerit:
            {
                var table = JoinTables(tablePages);
                meritTable = Table<MeritTableRow>.Create(
                    table.Headers,
                    table.Sections,
                    ParseMeritTable(table),
                    null,
                    null
                );
                break;
            }
            case PageEnum.IndexByCourse:
            {
                var tables = GetTables(tablePages);
                foreach (var table in tables)
                {
                    var courseTable = Table<CourseTableRow>.Create(
                        table.Headers,
                        table.Sections,
                        ParseCourseTable(table),
                        table.CourseTitle,
                        table.CourseLocation
                    );
                    courseTables.Add(courseTable);
                }

                break;
            }
            default:
                Console.WriteLine(
                    $"[ERROR] Unhandled sub index (url: {html.Url?.Url}, type: {html.Url?.PageEnum})"
                );
                break;
        }
    }

    private static HtmlPage? SubIndex(IEnumerable<HtmlPage> allHtmls, RankingUrl url)
    {
        bool FindUrlSimilar(HtmlPage? h)
        {
            if (h == null)
                return false;

            var urlUrl = h.Url?.Url;
            var s = url.Url;

            return CheckIfSimilar(urlUrl, s);
        }

        var subIndex = allHtmls.ToList().Find(FindUrlSimilar);
        return subIndex;
    }

    private static bool CheckIfSimilar(string? a, string b)
    {
        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            return true;

        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;

        a = a.Replace('\\', '/');
        b = b.Replace('\\', '/');

        if (!a.Contains('/') || !b.Contains('/'))
            return false;

        var aStrings = a.Split("/").Where(x => !string.IsNullOrEmpty(x) && x != "http:").ToList();
        var bStrings = b.Split("/").Where(x => !string.IsNullOrEmpty(x) && x != "http:").ToList();

        var min = Math.Min(aStrings.Count, bStrings.Count);
        aStrings = aStrings.Skip(Math.Max(0, aStrings.Count - min)).ToList();
        bStrings = bStrings.Skip(Math.Max(0, bStrings.Count - min)).ToList();

        for (var i = 0; i < min; i++)
            if (aStrings[i] != bStrings[i])
                return false;

        return true;
    }

    private static void AddRankingAndMerge(
        RankingsSet rankingsSet,
        Ranking ranking,
        bool forceReparsing
    )
    {
        bool Predicate(Ranking x)
        {
            return x.Url?.Url == ranking.Url?.Url;
        }

        lock (rankingsSet)
        {
            var savedRanking = rankingsSet.Rankings.Find(Predicate);
            if (savedRanking != null && !forceReparsing)
            {
                savedRanking.Merge(ranking);
                return;
            }

            if (savedRanking != null && forceReparsing)
                rankingsSet.Rankings.Remove(savedRanking);

            rankingsSet.AddRanking(ranking);
        }
    }

    private static IEnumerable<Table<List<string>>> GetTables(IEnumerable<HtmlPage> pages)
    {
        var tables = pages
            .Select(page =>
            {
                var isCourse = page.Url?.PageEnum == PageEnum.TableByCourse;
                var doc = page.Html?.DocumentNode;
                var header = GetTableHeader(doc);
                if (header is (null, null))
                    return null;

                var rows = doc?.SelectNodes("//table[contains(@class, 'TableDati')]/tbody/tr")
                    .ToList();
                var fullTitle = isCourse
                    ? doc?.GetElementsByClassName("titolo").ToList()[0].InnerText
                    : null;
                var title = isCourse ? fullTitle?.Split(" (")[0] : null;
                var location = isCourse ? GetCourseLocation(fullTitle) : null;
                var rowsData = rows?.Select(
                        row => row.Descendants("td").Select(node => node.InnerText).ToList()
                    )
                    .ToList();
                return Table.Create(header.Item1, header.Item2, rowsData, title, location);
            })
            .Where(el => el is not null)
            .ToList();

        return tables!;
    }

    private static string? GetCourseLocation(string? fullTitle)
    {
        var strings = fullTitle?.Split("(");
        if (strings == null)
            return null;
        if (strings.Length < 2)
            return null;
        var s = strings[1];
        var split = s.Split(")");
        return split[0];
    }

    private static (List<string>?, List<string>?) GetTableHeader(HtmlNode? doc)
    {
        var rows = doc?.SelectNodes("//table[contains(@class, 'TableDati')]/thead/tr");
        if (rows is null)
            return (null, null); // page invalid

        var badIndex = rows[0]
            .Descendants("th")
            .ToList()
            .FindIndex(node => node.GetAttributeValue("colSpan", 1) > 1);
        var rowsText = rows.Select(
                row =>
                    row.Descendants("th")
                        .Select(th => th.Descendants("#text").ToList()[0].InnerText)
                        .ToList()
            )
            .ToList();
        if (rows.Count == 1 || badIndex == -1)
            return (rowsText[0], null);

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

    private static bool EnrollCourseToAllowed(string? enrollCourse)
    {
        var lower = enrollCourse?.ToLower().Trim();

        if (string.IsNullOrEmpty(lower))
            return false;
        if (lower == "-")
            return false;
        string[] tester = { "immatricolazione non consentita", "non ammesso", "non idoneo" };
        return tester.All(test => !lower.Contains(test));
    }

    private static List<MeritTableRow> ParseMeritTable(Table<List<string>> table)
    {
        List<MeritTableRow> parsedRows = new();
        var headers = table.Headers.Select(h => h.ToLower()).ToList();

        var idIndex = headers.FindIndex(t => t.Contains("matricola") && !t.Contains("corso"));
        var votoTestIndex = headers.FindIndex(t => t.Contains("voto"));
        var posIndex = headers.FindIndex(t => t.Contains("posizione"));
        var corsoIndex = headers.FindIndex(t => t.Contains("corso") || t == "stato"); // "stato" per architettura
        var ofaEngIndex = headers.FindIndex(t => t.Contains("ofa inglese"));
        var ofaTestIndex = headers.FindIndex(t => t.Contains("ofa test"));

        foreach (var row in table.Data)
        {
            var id = HashMatricola.HashMatricolaMethod(Table.GetFieldByIndex(row, idIndex));
            var votoTest = Table.GetFieldByIndex(row, votoTestIndex) ?? "0";
            var enrollCourse = Table.GetFieldByIndex(row, corsoIndex) ?? "";
            var position = Table.GetFieldByIndex(row, posIndex) ?? "-1";
            var enrollAllowed = EnrollCourseToAllowed(enrollCourse);
            var ofa = new Dictionary<string, bool>();

            var ofaEng = Table.GetFieldByIndex(row, ofaEngIndex);
            if (ofaEng is not null)
                ofa.Add("ENG", ofaEng.ToLower().Contains("si"));

            var ofaTest = Table.GetFieldByIndex(row, ofaTestIndex);
            if (ofaTest is not null)
                ofa.Add("TEST", ofaTest.ToLower().Contains("si"));

            var votoString = votoTest.Replace(",", ".");
            var parsedRow = new MeritTableRow
            {
                Id = id,
                Position = Convert.ToInt16(position),
                Result = Convert.ToDecimal(votoString, Culture.NumberFormatInfo),
                Ofa = ofa,
                CanEnrollInto = enrollAllowed ? enrollCourse : null,
                CanEnroll = enrollAllowed
            };
            parsedRows.Add(parsedRow);
        }

        return parsedRows;
    }

    private static List<CourseTableRow> ParseCourseTable(Table<List<string>> table)
    {
        List<CourseTableRow> parsedRows = new();
        var headers = table.Headers.Select(h => h.ToLower()).ToList();

        var posIndex = headers.FindIndex(t => t.Contains("posizione"));
        var idIndex = headers.FindIndex(t => t.Contains("matricola") && !t.Contains("consentita"));
        var birthDateIndex = headers.FindIndex(t => t.Contains("nascita"));
        var enrollAllowedIndex = headers.FindIndex(t => t.Contains("consentita"));
        var votoTestIndex = headers.FindIndex(t => t.Contains("voto"));
        var ofaEngIndex = headers.FindIndex(t => t.Contains("ofa inglese"));
        var ofaTestIndex = headers.FindIndex(t => t.Contains("ofa test"));
        var englishCorrectAnswersIndex = headers.FindIndex(
            t => t.Contains("risposte esatte inglese")
        );

        var sectionsIndex = table.GetSectionsIndex();

        foreach (var row in table.Data)
            ParseRow(
                row,
                idIndex,
                votoTestIndex,
                posIndex,
                birthDateIndex,
                enrollAllowedIndex,
                englishCorrectAnswersIndex,
                ofaEngIndex,
                ofaTestIndex,
                sectionsIndex,
                parsedRows
            );
        return parsedRows;
    }

    private static void ParseRow(
        List<string> row,
        int idIndex,
        int votoTestIndex,
        int posIndex,
        int birthDateIndex,
        int enrollAllowedIndex,
        int englishCorrectAnswersIndex,
        int ofaEngIndex,
        int ofaTestIndex,
        Dictionary<string, int>? sectionsIndex,
        ICollection<CourseTableRow> parsedRows
    )
    {
        var id = HashMatricola.HashMatricolaMethod(Table.GetFieldByIndex(row, idIndex));
        var votoTestString = Table.GetFieldByIndex(row, votoTestIndex)?.Replace(",", ".") ?? "0";

        var votoTest = Convert.ToDecimal(votoTestString, Culture.NumberFormatInfo);
        var fieldByIndex = Table.GetFieldByIndex(row, posIndex) ?? "-1";
        if (fieldByIndex.ToLower().Contains("nessun"))
            return;

        var position = Convert.ToInt16(fieldByIndex);
        var birthDate = DateOnly.ParseExact(
            Table.GetFieldByIndex(row, birthDateIndex) ?? "",
            "dd/MM/yyyy"
        );
        var enrollAllowed =
            Table.GetFieldByIndex(row, enrollAllowedIndex)?.ToLower().Contains("si") ?? false;
        var englishCorrectAnswersValue = Table.GetFieldByIndex(row, englishCorrectAnswersIndex);
        int? englishCorrectAnswers = englishCorrectAnswersValue is not null
            ? Convert.ToInt16(englishCorrectAnswersValue)
            : null;
        var ofa = new Dictionary<string, bool>();

        var ofaEng = Table.GetFieldByIndex(row, ofaEngIndex);
        if (ofaEng is not null)
            ofa.Add("ENG", ofaEng.ToLower().Contains("si"));

        var ofaTest = Table.GetFieldByIndex(row, ofaTestIndex);
        if (ofaTest is not null)
            ofa.Add("TEST", ofaTest.ToLower().Contains("si"));

        var sectionsResults = new Dictionary<string, decimal>();
        if (sectionsIndex is not null)
            foreach (var section in sectionsIndex)
            {
                var resultString = row[section.Value].Replace(",", ".");
                var result = Convert.ToDecimal(resultString, Culture.NumberFormatInfo);
                sectionsResults.Add(section.Key, result);
            }

        var parsedRow = new CourseTableRow
        {
            Id = id,
            Position = position,
            Result = votoTest,
            Ofa = ofa,
            CanEnroll = enrollAllowed,
            EnglishCorrectAnswers = englishCorrectAnswers,
            BirthDate = birthDate,
            SectionsResults = sectionsResults
        };
        parsedRows.Add(parsedRow);
    }

    private static SchoolEnum GetSchoolEnum(string? schoolStr)
    {
        if (string.IsNullOrEmpty(schoolStr))
            return SchoolEnum.Unknown;

        if (schoolStr.Contains("design"))
            return SchoolEnum.Design;
        if (schoolStr.Contains("ingegneria") && !schoolStr.Contains("architettura"))
            return SchoolEnum.Ingegneria;
        if (schoolStr.Contains("architettura"))
            return SchoolEnum.Architettura;
        if (schoolStr.Contains("urbanistica"))
            return SchoolEnum.Urbanistica;
        return SchoolEnum.Unknown;
    }

    private static IEnumerable<HtmlPage> ParseLocalHtmlFiles(string htmlFolder)
    {
        HashSet<HtmlPage> elements = new();
        if (string.IsNullOrEmpty(htmlFolder))
            return elements;

        if (!Directory.Exists(htmlFolder))
            return elements;

        var files = Directory.GetFiles(htmlFolder, "*.html", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileSplit = file.Split(htmlFolder);

            var fileRelativePath = fileSplit[1];

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

    public static T? ParseJson<T>(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return default;

        var fileContent = File.ReadAllText(path);
        if (string.IsNullOrEmpty(fileContent))
            return default;

        var obj = JsonConvert.DeserializeObject<T>(fileContent, Culture.JsonSerializerSettings);
        return obj;
    }
}