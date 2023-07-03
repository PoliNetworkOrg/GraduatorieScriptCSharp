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
    internal const string PathCustom = "BySchoolYearCourse.json";

    //keys: school, year, phase, course, location
    public Dictionary<SchoolEnum, Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>> Schools = new();

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

            var schoolDict = new Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>();

            var byYears = schoolGroup.GroupBy(r => r.Year);
            foreach (var yearGroup in byYears)
            {
                if (yearGroup.Key is null)
                    continue;
                var filenames = yearGroup
                    .SelectMany(ranking => ranking.ToSingleCourseJson())
                    .DistinctBy(x => new {x.Link, x.Location})
                    .ToList().OrderBy(a => a.Name).ToList();
                var dict2 = ToDict(filenames, yearGroup);
                schoolDict.Add(yearGroup.Key.Value, dict2);
            }

            mainJson.Schools.Add(school, schoolDict);
        }

        return mainJson;
    }

    private static  Dictionary<string, Dictionary<string, List<SingleCourseJson>>> ToDict(List<SingleCourseJson> filenames,
        IGrouping<int?, Ranking> yearGroup)
    {
        var dictionary = new Dictionary<string, Dictionary<string, List<SingleCourseJson>>>();
        var coursesNames = yearGroup.ToList().SelectMany(x => x.ByCourse ?? new List<CourseTable>())
            .Select(x => x.Title).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        foreach (var courseName in coursesNames)
        {
            if (courseName == null) continue;

            var singleCourseJsons2 = SingleCourseJsonsGet(yearGroup, filenames);
            
            dictionary[courseName] = singleCourseJsons2;
        }

        return dictionary;
    }


    private static Dictionary<string, List<SingleCourseJson>> SingleCourseJsonsGet(
        IGrouping<int?, Ranking> yearGroup,
        List<SingleCourseJson> listCourses)
    {
        var singleCourseJsons = new Dictionary<string, List<SingleCourseJson>>();
        var courseJsons = listCourses.Where(x => IsSimilar(yearGroup, x)).ToList();
        foreach (SingleCourseJson variable in courseJsons)
        {
            string? location = variable.Location;
            if (!string.IsNullOrEmpty(location))
            {
                if (!singleCourseJsons.ContainsKey(location))
                    singleCourseJsons[location] = new List<SingleCourseJson>();

                singleCourseJsons[location].Add(variable);
            }
        }
        
        return singleCourseJsons;
    }

    private static bool IsSimilar(IEnumerable<Ranking> yearGroup, SingleCourseJson singleCourseJson)
    {
        var enumerable = yearGroup.Where(v1 => v1.ByCourse != null);

        bool Predicate(Ranking v1)
        {
            return singleCourseJson.School == v1.School && singleCourseJson.Year == v1.Year &&
                   v1.Phase == singleCourseJson.Name;
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

    private static void RankingsAddSingleYearSchool(KeyValuePair<int, Dictionary<string, Dictionary<string,List<SingleCourseJson>>>> year,
        string outFolder,
        KeyValuePair<SchoolEnum, Dictionary<int, Dictionary<string, Dictionary<string,List<SingleCourseJson>>>>> school,
        ICollection<Ranking> rankings)
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
        KeyValuePair<SchoolEnum, Dictionary<int, Dictionary<string, Dictionary<string,List<SingleCourseJson>>>>> school,
        KeyValuePair<int, Dictionary<string, Dictionary<string,List<SingleCourseJson>>>> year,
        string outFolder,
        KeyValuePair<string, List<SingleCourseJson>> filename,
        ICollection<Ranking> rankings)
    {
        foreach (var variable in filename.Value)
        {
             RankingAddSingle(school, year, outFolder, rankings, variable);
        }
    }

    private static void RankingAddSingle(KeyValuePair<SchoolEnum, Dictionary<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>>> school, KeyValuePair<int, Dictionary<string, Dictionary<string, List<SingleCourseJson>>>> year, string outFolder, ICollection<Ranking> rankings,
        SingleCourseJson variable)
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
        if (rankings.Any(x =>
                x.School == ranking.School && x.Year == ranking.Year && Similar(x.ByCourse, ranking.ByCourse)))
            return;

        rankings.Add(ranking);
    }

    private static bool Similar(IReadOnlyCollection<CourseTable>? a, IReadOnlyCollection<CourseTable>? b)
    {
        if (a == null || b == null)
            return false;
        return a.Count == b.Count && a.Select(variable => b.Any(x => x.Title == variable.Title)).All(boolB => boolB);
    }
}