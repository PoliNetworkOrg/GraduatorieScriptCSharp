using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearJson : IndexJsonBase
{
    internal const string PathCustom = "bySchoolYear.json";

    public Dictionary<SchoolEnum, Dictionary<int, IEnumerable<SingleCourseJson>>> Schools = new();

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

            var schoolDict = new Dictionary<int, IEnumerable<SingleCourseJson>>();

            var byYears = schoolGroup.GroupBy(r => r.Year);
            foreach (var yearGroup in byYears)
            {
                if (yearGroup.Key is null)
                    continue;
                var filenames = yearGroup
                    .SelectMany(ranking => ranking.ToSingleCourseJson())
                    .DistinctBy(x => x.Link)
                    .ToList().OrderBy(a => a.Name);
                schoolDict.Add(yearGroup.Key.Value, filenames);
            }

            mainJson.Schools.Add(school, schoolDict);
        }

        return mainJson;
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
        foreach (var school in mainJson.Schools)
        foreach (var year in school.Value)
        foreach (var filename in year.Value)
        {
            var path = Path.Join(outFolder, filename.BasePath, filename.Link);
            var ranking = Utils.Transformer.ParserNS.Parser.ParseJsonRanking(path);
            if (ranking != null) rankings.Add(ranking);
        }


        return rankings;
    }
}