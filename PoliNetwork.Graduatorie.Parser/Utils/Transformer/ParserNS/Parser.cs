#region

using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Extensions;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Common.Utils;
using PoliNetwork.Graduatorie.Common.Utils.HashNS;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;
using PoliNetwork.Graduatorie.Scraper.Utils.Web;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Transformer.ParserNS;

public class Parser
{
    private readonly ArgsConfig _config;
    private readonly string _htmlFolder;

    public Parser(ArgsConfig argsConfig)
    {
        _config = argsConfig;
        _htmlFolder = Path.Join(argsConfig.DataFolder, Constants.HtmlFolder);
    }

    public RankingsSet GetRankings(List<RankingUrl> urls)
    {
        // pseudo
        // parse saved html
        // fix url (replace \\ -> /)
        // get urls distinct (only where not in saved html) and make html
        // r1 = !forceReparse && parse from local Json
        // r2 = parse from new links if any
        // set = merge r1 and r2
        // order set by school => year => url (for git)
        // return
        //
        foreach (var url in urls) url.FixSlashes();

        var htmls = ParseLocalHtmlFiles().ToList();
        var newHtmls = urls.Where(url => htmls.All(h => h.Url.Url != url.Url))
            .DistinctBy(url => url.Url)
            .Select(url => HtmlPage.FromUrl(url, _htmlFolder))
            .Where(html => html != null)
            .Select(html => html!);

        htmls.AddRange(newHtmls);

        var savedSet = ParseSavedRankings(htmls);
        var newSet = ParseNewRankings(htmls);
        savedSet.Merge(newSet);

        return savedSet;
    }

    private RankingsSet ParseSavedRankings(ICollection<HtmlPage> htmls)
    {
        // pseudo
        // new ranking set
        // read data folder
        // parse single json into single ranking
        // put rankings into set
        // remove from param htmls the found ranking url-html
        // return
        //
        var savedSet = _config.ForceReparsing
            ? new RankingsSet()
            : BySchoolYearJson.GetAndParse(_config.DataFolder);

        // removing urls of rankings found
        foreach (var ranking in savedSet.Rankings)
        {
            if (ranking.Url == null)
                continue;

            ranking.Url.FixSlashes();
            var relatedHtmls = htmls.Where(h => h.Url.IsSameRanking(ranking.Url)).ToList();
            foreach (var related in relatedHtmls)
                lock (htmls)
                {
                    htmls.Remove(related);
                }
        }

        return savedSet;
    }

    private RankingsSet ParseNewRankings(IReadOnlyCollection<HtmlPage> htmls)
    {
        // pseudo
        // new ranking set
        // group urls into "Index", "IndexByMerit", "IndexByCourse", "TableMerit", "TableCourse"
        // iterate Index
        //  - find ranking info
        //  - search all IndexByMeritPage and all IndexByCourse
        //      => if not in upper variables, add them
        //      iterate IndexByMerit => Join tables into one of each page
        //      iterate IndexByCourse => Get tables from each page
        //  - build rankings with info, single merit table, multiple course table
        //  - add to ranking set
        // return set

        // here we have only the urls without saved ranking or all the urls if
        // _config.ForceReparsing is true

        var indexPages = htmls.Where(h => h.Url.PageEnum == PageEnum.Index);
        var meritTablePages = htmls.Where(h => h.Url.PageEnum == PageEnum.TableByMerit).ToList();
        var courseTablePages = htmls.Where(h => h.Url.PageEnum == PageEnum.TableByCourse).ToList();

        RankingsSet set = new();
        foreach (var index in indexPages)
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            var indexUrl = index.Url!;
            var doc = index.Html.DocumentNode;
            var ranking = InitRanking(indexUrl, doc);
            if (ranking == null)
                continue;

            var (ibMerit, ibCourse) = ParseIndexBy(indexUrl, doc);
            var meritPages = ibMerit
                .Select(url =>
                {
                    var found = meritTablePages.Find(h => h.Url.Url == url.Url);
                    return found ?? HtmlPage.FromUrl(url, _htmlFolder);
                })
                .Where(h => h != null)
                .Select(h => h!)
                .ToList();
            var meritTable = ParseMeritTable(meritPages);

            var coursesPages = ibCourse
                .Select(url =>
                {
                    var found = courseTablePages.Find(h => h.Url.Url == url.Url);
                    return found ?? HtmlPage.FromUrl(url, _htmlFolder);
                })
                .Where(h => h != null)
                .Select(h => h!)
                .ToList();
            var coursesTables = ParseCoursesTables(coursesPages);

            ranking.ByCourse = coursesTables
                .Select(
                    course =>
                        new CourseTable
                        {
                            Title = course.CourseTitle,
                            Location = course.CourseLocation,
                            Sections = course.Sections,
                            Headers = course.Headers,
                            Rows = GetCourseStudents(course, meritTable),
                            Year = ranking.Year,
                            Path = index.Url.Url
                        }
                )
                .OrderBy(x => x.Title)
                .ThenBy(x => x.Location)
                .ToList();

            ranking.ByMerit = new MeritTable
            {
                Year = ranking.Year,
                Path = index.Url.Url,
                Headers = meritTable.Headers,
                Rows = GetMeritStudents(meritTable, ranking.ByCourse)
            };

            ranking.RankingSummary = ranking.CreateSummary();
            set.Rankings.Add(ranking);
        }

        return set;
    }

    private static Ranking? InitRanking(RankingUrl indexUrl, HtmlNode doc)
    {
        var ranking = new Ranking();
        // get ranking info
        var intestazioni = doc.GetElementsByClassName("intestazione")
            .Select(i => i.Descendants("#text").ToList()[0].InnerText)
            .ToList();

        var schoolStr = intestazioni[2].Split("\n")[0].ToLower();
        var school = GetSchoolEnum(schoolStr);

        if (school == SchoolEnum.Unknown)
        {
            Console.WriteLine(
                $"[ERROR] School '{schoolStr}' not recognized (index: {indexUrl.Url}); skipped"
            );
            return null;
        }

        ranking.Url = indexUrl;
        ranking.School = school;
        ranking.Year = Convert.ToInt16(intestazioni[1].Split("Year ")[1].Split("/")[0]);

        if (ranking.Year < 2024) {
            // layout valid until 2023
            var phase = string.Join(" ", intestazioni[3].Split(" - ")[1..]);
            ranking.RankingOrder = new RankingOrder(phase);
            if (ranking.School == SchoolEnum.Architettura && ranking.RankingOrder.Primary == null &&
                ranking.RankingOrder.Secondary == null && ranking.RankingOrder.ExtraEu == true)
            {
                // this is a fallback for 2020-2023:
                // POLIMI was used to add the ranking number (Secondary, e.g. "Prima Graduatoria") for ExtraEU starting 
                // from the second ranking. 
                // e.g. Extra-EU first ranking => phase = "Extra-ue",
                //      Extra-EU second ranking => phase = "Extra-ue - Seconda Graduatoria"
                // so this is a fallback to add the equivalent of "Prima Graduatoria" to the first ExtraEU ranking.
                
                ranking.RankingOrder.Secondary = 1;
            }
        } else {
            // layout valid since 2024 (if the layout changes again, make another else if)
            var phase = intestazioni[3];
            var isEnglish = intestazioni[2].Contains("taught in english") || intestazioni[2].Contains("erogati in inglese");
            ranking.RankingOrder = new RankingOrder(phase, isEnglish);
        }

        ranking.Extra = intestazioni[4];
        ranking.LastUpdate = DateTime.UtcNow;
        ranking.ByCourse = new List<CourseTable>();

        return ranking;
    }

    private (IEnumerable<RankingUrl>, IEnumerable<RankingUrl>) ParseIndexBy(
        RankingUrl indexUrl,
        HtmlNode doc
    )
    {
        var aTags = doc.GetElementsByClassName("titolo")
            .SelectMany(a => a.GetElementsByTagName("a")) // links to subindex
            .Where(a => !a.InnerText.Contains("matricola"))
            .ToList(); // filter out id ranking

        var baseDomain = indexUrl.GetBaseDomain();

        var subUrls = aTags
            .Select(a => a.GetAttributeValue("href", null))
            .Where(href => href != null)
            .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
            .Select(RankingUrl.From)
            .Where(url => url.PageEnum is PageEnum.IndexByMerit or PageEnum.IndexByCourse)
            .ToList();

        var meritIndexUrl = subUrls.Find(u => u.PageEnum == PageEnum.IndexByMerit);
        var meritIndex =
            meritIndexUrl != null ? HtmlPage.FromUrl(meritIndexUrl, _htmlFolder) : null;
        var meritPages = meritIndex != null ? GetTableLinks(meritIndex) : new List<RankingUrl>();

        var courseIndexUrl = subUrls.Find(u => u.PageEnum == PageEnum.IndexByCourse);
        var courseIndex =
            courseIndexUrl != null ? HtmlPage.FromUrl(courseIndexUrl, _htmlFolder) : null;
        var coursesPages =
            courseIndex != null ? GetTableLinks(courseIndex) : new List<RankingUrl>();

        return (meritPages, coursesPages);
    }

    private static IEnumerable<RankingUrl> GetTableLinks(HtmlPage html)
    {
        var baseDomain = html.Url.GetBaseDomain();

        var page = html.Html.DocumentNode;
        var tablesLinks = page.SelectNodes("//td/a")
            .ToList()
            .Select(a => a.GetAttributeValue("href", null))
            .Where(href => href != null)
            .Select(href => UrlUtils.UrlifyLocalHref(href!, baseDomain))
            .Select(RankingUrl.From)
            .ToList();

        return tablesLinks;
    }

    private static Table<MeritTableRow> ParseMeritTable(IEnumerable<HtmlPage> pages)
    {
        var table = JoinTables(pages);
        var meritTable = Table<MeritTableRow>.Create(
            table.Headers,
            table.Sections,
            ParseMeritTable(table),
            null,
            null
        );

        return meritTable;
    }

    private static IEnumerable<Table<CourseTableRow>> ParseCoursesTables(IEnumerable<HtmlPage> pages)
    {
        var tables = GetTables(pages);
        var coursesTables = tables.Select(
            table =>
                Table<CourseTableRow>.Create(
                    table.Headers,
                    table.Sections,
                    ParseCourseTable(table),
                    table.CourseTitle,
                    table.CourseLocation
                )
        );

        return coursesTables;
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

    private static IEnumerable<Table<List<string>>> GetTables(IEnumerable<HtmlPage> pages)
    {
        var tables = pages
            .Select(page =>
            {
                var isCourse = page.Url.PageEnum == PageEnum.TableByCourse;
                var doc = page.Html.DocumentNode;

                var (headers, sections) = GetTableHeader(doc);
                if (headers == null)
                    return null;

                var rows = doc.SelectNodes("//table[contains(@class, 'TableDati')]/tbody/tr")
                    .ToList();

                var fullTitle = isCourse
                    ? doc.GetElementsByClassName("titolo").ToList()[0].InnerText
                    : null;

                var title = isCourse && fullTitle != null ? fullTitle.Split(" (")[0] : null;
                var location = isCourse && fullTitle != null ? GetCourseLocation(fullTitle) : null;
                var rowsData = rows.Select(
                        row => row.Descendants("td").Select(node => node.InnerText).ToList()
                    )
                    .ToList();
                return Table.Create(headers, sections, rowsData, title, location);
            })
            .Where(el => el is not null)
            .ToList();

        return tables!;
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

    private static string? GetCourseLocation(string fullTitle)
    {
        // ex: INGEGNERIA MECCANICA (MILANO LEONARDO)
        var strings = fullTitle.Split("("); // [INGEGNERIA MECCANICA, MILANO LEONARDO)]
        if (strings.Length < 2)
            return null;

        var s = strings[1]; // MILANO LEONARDO)
        var split = s.Split(")"); // [MILANO LEONARDO, ]
        return split[0]; // MILANO LEONARDO
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
            var ofa = new SortedDictionary<string, bool>();

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

        var parsedRows = table.Data
            .Select(
                row =>
                    ParseCourseRow(
                        row,
                        idIndex,
                        votoTestIndex,
                        posIndex,
                        birthDateIndex,
                        enrollAllowedIndex,
                        englishCorrectAnswersIndex,
                        ofaEngIndex,
                        ofaTestIndex,
                        sectionsIndex
                    )
            )
            .Where(row => row != null)
            .Select(row => row!)
            .ToList();

        return parsedRows;
    }

    private static CourseTableRow? ParseCourseRow(
        List<string> row,
        int idIndex,
        int votoTestIndex,
        int posIndex,
        int birthDateIndex,
        int enrollAllowedIndex,
        int englishCorrectAnswersIndex,
        int ofaEngIndex,
        int ofaTestIndex,
        SortedDictionary<string, int>? sectionsIndex
    )
    {
        var id = HashMatricola.HashMatricolaMethod(Table.GetFieldByIndex(row, idIndex));
        var votoTestString = Table.GetFieldByIndex(row, votoTestIndex)?.Replace(",", ".") ?? "0";

        var votoTest = Convert.ToDecimal(votoTestString, Culture.NumberFormatInfo);
        var fieldByIndex = Table.GetFieldByIndex(row, posIndex) ?? "-1";
        if (fieldByIndex.ToLower().Contains("nessun"))
            return null;

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
        var ofa = new SortedDictionary<string, bool>();

        var ofaEng = Table.GetFieldByIndex(row, ofaEngIndex);
        if (ofaEng is not null)
            ofa.Add("ENG", ofaEng.ToLower().Contains("si"));

        var ofaTest = Table.GetFieldByIndex(row, ofaTestIndex);
        if (ofaTest is not null)
            ofa.Add("TEST", ofaTest.ToLower().Contains("si"));

        var sectionsResults = new SortedDictionary<string, decimal>();
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
        return parsedRow;
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
            EnrollType = EnrollUtil.GetEnrollType(course.CourseTitle, canEnroll),
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
        student.EnrollType = EnrollUtil.GetEnrollType(meritRow.CanEnrollInto, canEnroll);
        return student;
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
            EnrollType = EnrollUtil.GetEnrollType(row.CanEnrollInto, canEnroll),
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
            ? studentCoursesRows.Find(c => c?.EnrollType?.CanEnroll ?? false)
            : studentCoursesRows.OrderBy(c => c?.PositionCourse).First();

        if (finalRow == null)
            return student;

        student.PositionCourse = finalRow.PositionCourse;
        student.EnglishCorrectAnswers = finalRow.EnglishCorrectAnswers;
        student.SectionsResults = finalRow.SectionsResults;
        student.BirthDate = finalRow.BirthDate;
        return student;
    }

    private IEnumerable<HtmlPage> ParseLocalHtmlFiles()
    {
        Console.WriteLine($"[DEBUG] Started ParseLocalHtmlFiles {DateTime.Now}");

        HashSet<HtmlPage> elements = new();
        if (!Directory.Exists(_htmlFolder))
            return elements;

        var files = Directory.GetFiles(_htmlFolder, "*.html", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileSplit = file.Split(_htmlFolder);

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

        Console.WriteLine($"[DEBUG] Ended ParseLocalHtmlFiles {DateTime.Now}");
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

    public static Ranking? ParseJsonRanking(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return default;

        var fileContent = File.ReadAllText(path);
        if (string.IsNullOrEmpty(fileContent))
            return default;

        var deserializeObject = JsonConvert.DeserializeObject(
            fileContent,
            Culture.JsonSerializerSettings
        );
        if (deserializeObject == null)
            return null;

        var objectToRead = (JObject)deserializeObject;

        const string propertyName = "phase";
        if (objectToRead.TryGetValue(propertyName, out var jToken))
        {
            var obj1 = JsonConvert.DeserializeObject<Ranking?>(
                fileContent,
                Culture.JsonSerializerSettings
            );
            if (obj1 == null)
                return null;

            if (obj1.RankingOrder != null)
                return obj1;

            var jValue = (JValue)jToken;
            var phase = jValue.Value?.ToString();
            if (!string.IsNullOrEmpty(phase))
                obj1.RankingOrder = new RankingOrder(phase);

            return obj1;
        }

        var obj2 = JsonConvert.DeserializeObject<Ranking?>(
            fileContent,
            Culture.JsonSerializerSettings
        );
        return obj2;
    }
}
