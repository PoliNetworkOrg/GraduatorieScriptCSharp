#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

using YearsDict = SortedDictionary<int, SortedDictionary<SchoolEnum, IEnumerable<SingleCourseJson>>>;
using SchoolsDict = SortedDictionary<SchoolEnum, IEnumerable<SingleCourseJson>>;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ByYearSchoolJson : IndexJsonBase
{
    internal const string CustomPath = "byYearSchool.json";
    public List<SingleCourseJson> All = new(); // decide whether to include it in the json serialization

    public YearsDict Years = new();

    public static ByYearSchoolJson From(RankingsSet set)
    {
        var mainJson = new ByYearSchoolJson { LastUpdate = set.LastUpdate };

        var list = set.Rankings
            .SelectMany(r => r.ToSingleCourseJson()).ToList()
            .DistinctBy(r => new { r.Id })
            .ToList();

        list.Sort();
        mainJson.All = list;

        // group rankings by year
        var byYear = set.Rankings.GroupBy(r => r.Year);
        foreach (var yearGroup in byYear)
        {
            var year = yearGroup.Key;
            var bySchools = yearGroup.GroupBy(r => r.School);

            var schoolsDict = GetSchoolsDict(bySchools);
            mainJson.Years.Add(year, schoolsDict);
        }

        return mainJson;
    }

    private static SchoolsDict GetSchoolsDict(IEnumerable<IGrouping<SchoolEnum, Ranking>> bySchools)
    {
        var schoolsDict = new SchoolsDict();
        foreach (var schoolGroup in bySchools)
        {
            var filenames = schoolGroup
                .SelectMany(ranking => ranking.ToSingleCourseJson())
                .DistinctBy(x => x.Link)
                .OrderBy(r => r.Id)
                .ToList();

            schoolsDict.Add(schoolGroup.Key, filenames);
        }

        return schoolsDict;
    }
}