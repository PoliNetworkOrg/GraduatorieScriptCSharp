#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

using SchoolsDict = SortedDictionary<SchoolEnum, SortedDictionary<int, IEnumerable<SingleCourseJson>>>;
using YearsDict = SortedDictionary<int, IEnumerable<SingleCourseJson>>;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearJson : IndexJsonBase
{
    internal const string CustomPath = "bySchoolYear.json";

    public SchoolsDict Schools = new();
    public List<SingleCourseJson> All = new(); // decide whether to include it in the json serialization

    public static BySchoolYearJson From(RankingsSet set)
    {
        var mainJson = new BySchoolYearJson { LastUpdate = set.LastUpdate };

        var list = set.Rankings
            .SelectMany(r => r.ToSingleCourseJson())
            .DistinctBy(r => new { r.Id })
            .ToList();
        
        list.Sort();
        mainJson.All = list;

        // group rankings by school
        var bySchool = set.Rankings.Where(r => r.School != null).GroupBy(r => r.School!.Value);
        foreach (var schoolGroup in bySchool)
        {
            var school = schoolGroup.Key;
            var byYears = schoolGroup.Where(r => r.Year != null).GroupBy(r => r.Year!.Value);

            var yearsDict = GetYearsDict(byYears);
            mainJson.Schools.Add(school, yearsDict);
        }

        return mainJson;
    }

    private static YearsDict GetYearsDict(IEnumerable<IGrouping<int, Ranking>> byYears)
    {
        var yearsDict = new YearsDict();

        foreach (var yearGroup in byYears)
        {
            var singleCourseJsons = yearGroup
                .SelectMany(r => r.ToSingleCourseJson())
                .DistinctBy(r => r.Id)
                .OrderBy(e => e.Id) // Id contains everything (school, year, pri/sec phase, extraeu, lang)
                .ToList();

            yearsDict.Add(yearGroup.Key, singleCourseJsons);
        }

        return yearsDict;
    }

    public static RankingsSet GetAndParse(string dataFolder)
    {
        var set = new RankingsSet();
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, CustomPath);
        try
        {
            var index = Utils.Transformer.ParserNS.Parser.ParseJson<BySchoolYearJson>(mainJsonPath);
            if (index is null) return set;

            set.LastUpdate = index.LastUpdate;
            set.Rankings = index.GetRankings(outFolder);
            set.Rankings.Sort();
            return set;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e}");
            return set;
        }
    }

    public List<Ranking> GetRankings(string outFolder)
    {
        List<Ranking> rankings = new();

        foreach (var singleCourseJson in All)
        {
            var fullPath = singleCourseJson.GetFullPath(outFolder);
            var ranking = Ranking.FromJson(fullPath);
            if (ranking != null) rankings.Add(ranking);
        }

        return rankings;
    }
}