using GraduatorieCommon.Enums;
using GraduatorieCommon.Utils.ParallelNS;
using GraduatorieScript.Data;
using GraduatorieScript.Objects.RankingNS;
using GraduatorieScript.Utils.Transformer.ParserNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Indexes.Specific;

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


    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, PathCustom);
        try
        {
            var mainJson = Parser.ParseJson<BySchoolYearJson>(mainJsonPath);
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

    private static List<Ranking> RankingsAdd(BySchoolYearJson mainJson, string outFolder)
    {
        List<Ranking> rankings = new();
        foreach (var school in mainJson.Schools)
        foreach (var year in school.Value)
            RankingsAddSingleYearSchool(year, school, outFolder, rankings);

        return rankings;
    }

    private static void RankingsAddSingleYearSchool(KeyValuePair<int, IEnumerable<SingleCourseJson>> year,
        KeyValuePair<SchoolEnum, Dictionary<int, IEnumerable<SingleCourseJson>>> school, string outFolder,
        ICollection<Ranking> rankings)
    {
        Action Selector(SingleCourseJson filename)
        {
            return () => { RankingAdd(school, year, outFolder, filename, rankings); };
        }

        var actions = year.Value.Select(Selector).ToArray();
        ParallelRun.Run(actions);
    }

    private static void RankingAdd(
        KeyValuePair<SchoolEnum, Dictionary<int, IEnumerable<SingleCourseJson>>> school,
        KeyValuePair<int, IEnumerable<SingleCourseJson>> year,
        string outFolder,
        SingleCourseJson filename,
        ICollection<Ranking> rankings)
    {
        var schoolKey = school.Key.ToString();
        var yearKey = year.Key.ToString();
        var path = Path.Join(outFolder, schoolKey, yearKey, filename.Link);
        var ranking = Parser.ParseJson<Ranking>(path);
        if (ranking == null)
            return;

        lock (rankings)
        {
            rankings.Add(ranking);
        }
    }
}