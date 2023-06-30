using GraduatorieScript.Data.Constants;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.RankingNS;
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

    public Dictionary<SchoolEnum, Dictionary<int, IEnumerable<SingleCourseJson>>> Schools = new();

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

            var schoolDict = new Dictionary<int, IEnumerable<SingleCourseJson>>();

            var byYears = schoolGroup.GroupBy(r => r.Year);
            foreach (var yearGroup in byYears)
            {
                if (yearGroup.Key is null)
                    continue;
                var filenames = yearGroup
                    .Select(ranking => ranking.ToSingleCourseJson())
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
        {
            var actions = new List<Action>();
            foreach (var filename in year.Value)
                actions.Add(() => { RankingAdd(school, year, outFolder, filename, rankings); });

            ParallelRun.Run(actions.ToArray());
        }

        return rankings;
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
        if (ranking != null)
            lock (rankings)
            {
                rankings.Add(ranking);
            }
    }
}