#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearJson : IndexJsonBase
{
    internal const string PathCustom = "bySchoolYear.json";

    public SortedDictionary<SchoolEnum, SortedDictionary<int, IEnumerable<SingleCourseJson>>> Schools = new();

    public static BySchoolYearJson? From(RankingsSet? set)
    {
        if (set == null)
            return null;

        var mainJson = new BySchoolYearJson { LastUpdate = set.LastUpdate };
        // group rankings by school
        var bySchool = set.Rankings.GroupBy(r => r.School);
        foreach (var schoolGroup in bySchool)
        {
            if (schoolGroup.Key is null)
                continue;
            var school = schoolGroup.Key.Value;

            var schoolDict = new SortedDictionary<int, IEnumerable<SingleCourseJson>>();

            var byYears = schoolGroup.GroupBy(r => r.Year);
            foreach (var yearGroup in byYears)
            {
                if (yearGroup.Key is null)
                    continue;
                AddSchool(yearGroup, schoolDict);
            }

            mainJson.Schools.Add(school, schoolDict);
        }

        return mainJson;
    }

    private static void AddSchool(
        IGrouping<int?, Ranking> yearGroup,
        IDictionary<int, IEnumerable<SingleCourseJson>> schoolDict
    )
    {
        var yearGroupKey = yearGroup.Key;
        if (yearGroupKey == null)
            return;

        var singleCourseJsons = yearGroup
            .SelectMany(ranking => ranking.ToSingleCourseJson())
            .DistinctBy(x => x.Link)
            .ToList();
        var filenames = singleCourseJsons
            .OrderBy(a => a.Id)
            .ThenBy(a => a.Year)
            .ThenBy(a => a.School)
            .ThenBy(a => a.BasePath)
            .ToList();

        schoolDict.Add(yearGroupKey.Value, filenames);
    }


    public static RankingsSet GetAndParse(string dataFolder)
    {
        var set = new RankingsSet();
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, PathCustom);
        try
        {
            var mainJson = Utils.Transformer.ParserNS.Parser.ParseJson<BySchoolYearJson>(mainJsonPath);
            if (mainJson is null) return set;

            set.LastUpdate = mainJson.LastUpdate;
            set.Rankings = GetRankingsFromIndex(mainJson, outFolder);
            set.Rankings.Sort();
            return set;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e}");
            return set;
        }
    }

    private static List<Ranking> GetRankingsFromIndex(BySchoolYearJson mainJson, string outFolder)
    {
        List<Ranking> rankings = new();
        var singleCourseJsons = GetSingleCourseJsons(mainJson).ToList();
        singleCourseJsons.Sort();
        foreach (var filename in singleCourseJsons)
            AddRanking(outFolder, filename, rankings);

        return rankings;
    }

    private static IEnumerable<SingleCourseJson> GetSingleCourseJsons(BySchoolYearJson mainJson)
    {
        var singleCourseJsons = mainJson.Schools.SelectMany(
            school =>
            {
                var courseJsons = school.Value.SelectMany(year =>
                {
                    var yearValue = year.Value;
                    return yearValue;
                });
                return courseJsons;
            });
        return singleCourseJsons;
    }

    private static void AddRanking(string outFolder, SingleCourseJson filename, ICollection<Ranking> rankings)
    {
        var path = Path.Join(outFolder, filename.BasePath, filename.Link);
        var ranking = Utils.Transformer.ParserNS.Parser.ParseJsonRanking(path);
        if (ranking == null) return;
        rankings.Add(ranking);
    }
}