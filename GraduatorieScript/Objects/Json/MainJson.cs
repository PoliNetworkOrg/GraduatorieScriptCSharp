using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Utils.Transformer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MainJson
{
    public DateTime? LastUpdate;
    public Dictionary<SchoolEnum, Dictionary<int, IEnumerable<SingleCourseJson>>> Schools = new();

    public static void Write(string outFolder, RankingsSet set)
    {
        var mainJson = Generate(set, outFolder);
        mainJson.WriteToFile(outFolder);
    }

    private static MainJson Generate(RankingsSet set, string outFolder)
    {
        var mainJson = new MainJson { LastUpdate = set.LastUpdate };
        // group rankings by year
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
                var year = yearGroup.Key.Value;
                var folder = Path.Join(outFolder, school.ToString(), year.ToString());
                Directory.CreateDirectory(folder);

                foreach (var ranking in yearGroup)
                {
                    var path = Path.Join(folder, ranking.ConvertPhaseToFilename());
                    var rankingJsonString = JsonConvert.SerializeObject(ranking, Formatting.Indented);
                    File.WriteAllText(path, rankingJsonString);
                }

                var filenames = yearGroup.Select(ranking => ranking.ToSingleCourseJson()).DistinctBy(x => x.Link)
                    .ToList().OrderBy(a => a.Name);
                schoolDict.Add(year, filenames);
            }

            mainJson.Schools.Add(school, schoolDict);
        }

        return mainJson;
    }

    private void WriteToFile(string outFolder)
    {
        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        var mainJsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }

    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        try
        {
            var mainJson = Parser.ParseJson<MainJson>(mainJsonPath);
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

    private static List<Ranking> RankingsAdd(MainJson mainJson, string outFolder)
    {
        List<Ranking> rankings = new();
        foreach (var school in mainJson.Schools)
        foreach (var year in school.Value)
        foreach (var filename in year.Value)
            RankingAdd(school, year, outFolder, filename, rankings);

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
            rankings.Add(ranking);
    }
}