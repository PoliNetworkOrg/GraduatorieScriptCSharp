using GraduatorieScript.Data.Constants;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.RankingNS;
using GraduatorieScript.Utils.Transformer.ParserNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Indexes.Specific;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ByYearSchoolJson : IndexJsonBase
{
    internal const string PathCustom = "byYearSchool.json";

    public Dictionary<int, Dictionary<SchoolEnum, IEnumerable<SingleCourseJson>>> Years = new();

    public static ByYearSchoolJson? From(RankingsSet? set)
    {
        if (set == null)
            return null;

        var mainJson = new ByYearSchoolJson { LastUpdate = set.LastUpdate };
        // group rankings by year
        var byYear = set.Rankings.GroupBy(r => r.Year);
        foreach (var yearGroup in byYear)
        {
            if (yearGroup.Key is null)
                continue;
            var year = yearGroup.Key.Value;

            var yearDict = new Dictionary<SchoolEnum, IEnumerable<SingleCourseJson>>();

            var bySchools = yearGroup.GroupBy(r => r.School);
            foreach (var schoolGroup in bySchools)
            {
                if (schoolGroup.Key is null)
                    continue;
                var filenames = schoolGroup
                    .Select(ranking => ranking.ToSingleCourseJson())
                    .DistinctBy(x => x.Link)
                    .ToList().OrderBy(a => a.Name);
                yearDict.Add(schoolGroup.Key.Value, filenames);
            }

            mainJson.Years.Add(year, yearDict);
        }

        return mainJson;
    }


    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, PathCustom);
        try
        {
            var mainJson = Parser.ParseJson<ByYearSchoolJson>(mainJsonPath);
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

    private static List<Ranking> RankingsAdd(ByYearSchoolJson mainJson, string outFolder)
    {
        List<Ranking> rankings = new();
        foreach (var year in mainJson.Years)
        foreach (var school in year.Value)
        foreach (var filename in school.Value)
            RankingAdd(year, school, outFolder, filename, rankings);

        return rankings;
    }

    private static void RankingAdd(
        KeyValuePair<int, Dictionary<SchoolEnum, IEnumerable<SingleCourseJson>>> year,
        KeyValuePair<SchoolEnum, IEnumerable<SingleCourseJson>> school,
        string outFolder,
        SingleCourseJson filename,
        ICollection<Ranking> rankings)
    {
        var schoolKey = school.Key.ToString();
        var yearKey = year.Key.ToString();
        var path = Path.Join(outFolder, schoolKey, yearKey, filename.Link);
        var ranking = Parser.ParseJson<Ranking>(path);
        if (ranking != null)
            rankings.Add(ranking);
    }
}