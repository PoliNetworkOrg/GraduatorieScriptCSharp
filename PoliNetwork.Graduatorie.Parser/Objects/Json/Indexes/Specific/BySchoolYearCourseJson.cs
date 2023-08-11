#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Utils.ParallelNS;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearCourseJson : IndexJsonBase
{
    internal const string PathCustom = "bySchoolYearCourse.json";

    //keys: school, year, course, location
    public SortedDictionary<
        SchoolEnum,
        SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>
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

    private static SortedDictionary<
        int,
        SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>
    > GetYearsDict(IEnumerable<IGrouping<int?, Ranking>> byYears)
    {
        var d =
            new SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>();

        foreach (var yearGroup in byYears) GetYearsDictSingle(yearGroup, d);

        return d;
    }

    private static void GetYearsDictSingle(IGrouping<int?, Ranking> yearGroup,
        SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>> d)
    {
        if (yearGroup.Key != null) d.Add(yearGroup.Key.Value, GetCoursesDict(yearGroup));
    }

    private static SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>> GetCoursesDict(
        IEnumerable<Ranking> yearGroup
    )
    {
        var d = new SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>();

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
        SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>> d,
        Ranking ranking,
        IGrouping<string?, CourseTable> courseGroup
    )
    {
        var title = courseGroup.Key;
        if (string.IsNullOrEmpty(title))
            return;

        if (!d.ContainsKey(title))
            d[title] = new SortedDictionary<string, List<SingleCourseJson>>();

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

            if (locationDict.Any(IsThisCourse))
                continue;

            locationDict.Add(singleCourseJson);
            locationDict.Sort(Comparison);
            continue;

            bool IsThisCourse(SingleCourseJson x)
            {
                return x.Link == singleCourseJson.Link && x.Location == singleCourseJson.Location;
            }
        }
    }

    private static int Comparison(SingleCourseJson x, SingleCourseJson y)
    {
        return x.Compare(y);
    }

    private static SingleCourseJson CreateCourseJson(Ranking ranking, CourseTable course)
    {
        var basePath = ranking.School + "/" + ranking.Year + "/";
        return new SingleCourseJson
        {
            Link = ranking.ConvertPhaseToFilename(),
            Name = ranking.RankingOrder?.Phase,
            BasePath = basePath,
            Year = ranking.Year,
            School = ranking.School,
            Location = course.Location
        };
    }

    private static bool IsSimilar(IEnumerable<Ranking> yearGroup, SingleCourseJson singleCourseJson)
    {
        var enumerable = yearGroup.Where(v1 => v1.ByCourse != null);

        return enumerable.Any(Predicate);

        bool Predicate(Ranking v1)
        {
            return singleCourseJson.School == v1.School
                   && singleCourseJson.Year == v1.Year
                   && v1.RankingOrder?.Phase == singleCourseJson.Name;
        }
    }

    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, PathCustom);
        try
        {
            var mainJson = Utils.Transformer.ParserNS.Parser.ParseJson<BySchoolYearCourseJson>(mainJsonPath);
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
        KeyValuePair<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>> year,
        string outFolder,
        KeyValuePair<
            SchoolEnum,
            SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>
        > school,
        ICollection<Ranking> rankings
    )
    {
        var actions = new List<Action>();
        foreach (var filename in year.Value)
        {
            var collection = filename.Value.Select(Selector);
            actions.AddRange(collection);
            continue;

            Action Selector(KeyValuePair<string, List<SingleCourseJson>> variable)
            {
                return () => { RankingAdd(school, year, outFolder, variable, rankings); };
            }
        }

        ParallelRun.Run(actions.ToArray());
    }

    private static void RankingAdd(
        KeyValuePair<
            SchoolEnum,
            SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>
        > school,
        KeyValuePair<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>> year,
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
            SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>
        > school,
        KeyValuePair<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>> year,
        string outFolder,
        ICollection<Ranking> rankings,
        SingleCourseJson variable
    )
    {
        var schoolKey = school.Key.ToString();
        var yearKey = year.Key.ToString();
        var path = Path.Join(outFolder, schoolKey, yearKey, variable.Link);
        var ranking = Utils.Transformer.ParserNS.Parser.ParseJsonRanking(path);
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