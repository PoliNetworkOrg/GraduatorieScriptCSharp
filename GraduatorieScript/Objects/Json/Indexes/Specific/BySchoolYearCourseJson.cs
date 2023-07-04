using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.RankingNS;
using GraduatorieScript.Objects.Tables.Course;
using GraduatorieScript.Utils;
using GraduatorieScript.Utils.Transformer.ParserNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Indexes.Specific;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearCourseJson : IndexJsonBase
{
    internal const string PathCustom = "bySchoolYearCourse.json";

    //keys: school, year, course, location
    public Dictionary<
        SchoolEnum,
        Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>
    > Schools = new();

    public static BySchoolYearCourseJson? From(RankingsSet? set)
    {
        if (set == null)
            return null;

        var mainJson = new BySchoolYearCourseJson { LastUpdate = set.LastUpdate };
        // group rankings by school
        var bySchool = set.Rankings.GroupBy(r => r.School);
        foreach (var schoolGroup in bySchool)
        {
            if (schoolGroup.Key is null)
                continue;
            var school = schoolGroup.Key.Value;

            var byYears = schoolGroup.GroupBy(r => r.Year);
            var yearsDict = GetYearsDict(byYears);

            mainJson.Schools.Add(school, yearsDict);
        }

        return mainJson;
    }

    private static Dictionary<
        int,
        Dictionary<string, Dictionary<string, List<SingleCourseJson>>>
    > GetYearsDict(IEnumerable<IGrouping<int?, Ranking>> byYears)
    {
        var d =
            new Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>();

        foreach (var yearGroup in byYears) GetYearsDictSingle(yearGroup, d);

        return d;
    }

    private static void GetYearsDictSingle(IGrouping<int?, Ranking> yearGroup,
        IDictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>> d)
    {
        if (yearGroup.Key != null) d.Add(yearGroup.Key.Value, GetCoursesDict(yearGroup));
    }

    private static Dictionary<string, Dictionary<string, List<SingleCourseJson>>> GetCoursesDict(
        IEnumerable<Ranking> yearGroup
    )
    {
        var d = new Dictionary<string, Dictionary<string, List<SingleCourseJson>>>();

        foreach (var ranking in yearGroup)
        {
            if (ranking.ByCourse == null)
                continue;

            var byTitle = ranking.ByCourse.GroupBy(c => c.Title);
            foreach (var courseGroup in byTitle)
                AddCourseToDict(d, ranking, courseGroup);
        }

        return d;
    }

    private static void AddCourseToDict(
        IDictionary<string, Dictionary<string, List<SingleCourseJson>>> d,
        Ranking ranking,
        IGrouping<string?, CourseTable> courseGroup
    )
    {
        var title = courseGroup.Key;
        if (string.IsNullOrEmpty(title))
            return;

        if (!d.ContainsKey(title))
            d[title] = new Dictionary<string, List<SingleCourseJson>>();

        var courseDict = d[title];
        foreach (var course in courseGroup)
        {
            var location = course.Location;

            // fixedLocation
            // esempio: Urbanistica 2022 ha un solo corso senza location, ma anche quello
            // deve comparire nella lista
            // fix: se un corso non ha location, si inserisce un valore 0
            var fixedLocation = string.IsNullOrEmpty(location) ? "0" : location;

            if (!courseDict.ContainsKey(fixedLocation))
                courseDict[fixedLocation] = new List<SingleCourseJson>();

            var locationDict = courseDict[fixedLocation];
            var singleCourseJson = CreateCourseJson(ranking, course);

            bool IsThisCourse(SingleCourseJson x)
            {
                return x.Link == singleCourseJson.Link && x.Location == singleCourseJson.Location;
            }

            if (locationDict.Any(IsThisCourse))
                continue;

            locationDict.Add(singleCourseJson);
        }
    }

    private static SingleCourseJson CreateCourseJson(Ranking ranking, CourseTable course)
    {
        var basePath = ranking.School + "/" + ranking.Year + "/";
        return new SingleCourseJson
        {
            Link = ranking.ConvertPhaseToFilename(),
            Name = ranking.Phase,
            BasePath = basePath,
            Year = ranking.Year,
            School = ranking.School,
            Location = course.Location
        };
    }

    private static bool IsSimilar(IEnumerable<Ranking> yearGroup, SingleCourseJson singleCourseJson)
    {
        var enumerable = yearGroup.Where(v1 => v1.ByCourse != null);

        bool Predicate(Ranking v1)
        {
            return singleCourseJson.School == v1.School
                   && singleCourseJson.Year == v1.Year
                   && v1.Phase == singleCourseJson.Name;
        }

        return enumerable.Any(Predicate);
    }

    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, PathCustom);
        try
        {
            var mainJson = Parser.ParseJson<BySchoolYearCourseJson>(mainJsonPath);
            if (mainJson is null)
                return null;

            var rankings = RankingsAdd(mainJson, outFolder);

            return new RankingsSet { LastUpdate = mainJson.LastUpdate, Rankings = rankings };
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static List<Ranking> RankingsAdd(BySchoolYearCourseJson mainJson, string outFolder)
    {
        List<Ranking> rankings = new();
        foreach (var school in mainJson.Schools)
        foreach (var year in school.Value)
            RankingsAddSingleYearSchool(year, outFolder, school, rankings);

        return rankings;
    }

    private static void RankingsAddSingleYearSchool(
        KeyValuePair<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>> year,
        string outFolder,
        KeyValuePair<
            SchoolEnum,
            Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>
        > school,
        ICollection<Ranking> rankings
    )
    {
        var actions = new List<Action>();
        foreach (var filename in year.Value)
        {
            Action Selector(KeyValuePair<string, List<SingleCourseJson>> variable)
            {
                return () => { RankingAdd(school, year, outFolder, variable, rankings); };
            }

            var collection = filename.Value.Select(Selector);
            actions.AddRange(collection);
        }

        ParallelRun.Run(actions.ToArray());
    }

    private static void RankingAdd(
        KeyValuePair<
            SchoolEnum,
            Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>
        > school,
        KeyValuePair<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>> year,
        string outFolder,
        KeyValuePair<string, List<SingleCourseJson>> filename,
        ICollection<Ranking> rankings
    )
    {
        foreach (var variable in filename.Value)
            RankingAddSingle(school, year, outFolder, rankings, variable);
    }

    private static void RankingAddSingle(
        KeyValuePair<
            SchoolEnum,
            Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>
        > school,
        KeyValuePair<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>> year,
        string outFolder,
        ICollection<Ranking> rankings,
        SingleCourseJson variable
    )
    {
        var schoolKey = school.Key.ToString();
        var yearKey = year.Key.ToString();
        var path = Path.Join(outFolder, schoolKey, yearKey, variable.Link);
        var ranking = Parser.ParseJson<Ranking>(path);
        if (ranking == null)
            return;

        lock (rankings)
        {
            AddToRankings(rankings, ranking);
        }
    }

    private static void AddToRankings(ICollection<Ranking> rankings, Ranking ranking)
    {
        if (
            rankings.Any(
                x =>
                    x.School == ranking.School
                    && x.Year == ranking.Year
                    && Similar(x.ByCourse, ranking.ByCourse)
            )
        )
            return;

        rankings.Add(ranking);
    }

    private static bool Similar(
        IReadOnlyCollection<CourseTable>? a,
        IReadOnlyCollection<CourseTable>? b
    )
    {
        if (a == null || b == null)
            return false;
        return a.Count == b.Count
               && a.Select(variable => b.Any(x => x.Title == variable.Title)).All(boolB => boolB);
    }
}