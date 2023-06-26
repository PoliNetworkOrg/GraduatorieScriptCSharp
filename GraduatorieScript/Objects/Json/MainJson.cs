using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Utils.Transformer;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MainJson
{
    public DateTime? LastUpdate;
    public Dictionary<int, Dictionary<SchoolEnum, IEnumerable<SingleCourseJson>>> Years = new();

    public static void Write(string outFolder, RankingsSet set)
    {
        var mainJson = Generate(set, outFolder);
        mainJson.WriteToFile(outFolder);
    }

    private static MainJson Generate(RankingsSet set, string outFolder)
    {
        var mainJson = new MainJson
        {
            LastUpdate = set.LastUpdate
        };
        // group rankings by year
        var byYears = set.Rankings.GroupBy(r => r.Year);
        foreach (var yearGroup in byYears)
        {
            if (yearGroup.Key is null) continue;
            var year = yearGroup.Key.Value;
            var bySchool = yearGroup.GroupBy(r => r.School);

            var yearDict = new Dictionary<SchoolEnum, IEnumerable<SingleCourseJson>>();

            foreach (var schoolGroup in bySchool)
            {
                if (schoolGroup.Key is null) continue;
                var school = schoolGroup.Key.Value;
                var folder = Path.Join(outFolder, year.ToString(), school.ToString());
                Directory.CreateDirectory(folder);

                foreach (var ranking in schoolGroup)
                {
                    var path = Path.Join(folder, ranking.ConvertPhaseToFilename());
                    var rankingJsonString = JsonConvert.SerializeObject(ranking);
                    File.WriteAllText(path, rankingJsonString);
                }

                var filenames = schoolGroup.Select(ranking => ranking.ToSingleCourseJson());
                yearDict.Add(school, filenames);
            }

            mainJson.Years.Add(year, yearDict);
        }

        return mainJson;
    }

    private void WriteToFile(string outFolder)
    {
        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        var mainJsonString = JsonConvert.SerializeObject(this);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }

    public static RankingsSet? Parse(string dataFolder)
    {
        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        try
        {
            var mainJson = Parser.ParseJson<MainJson>(mainJsonPath);
            if (mainJson is null) return null;

            List<Ranking> rankings = new();
            foreach (var year in mainJson.Years)
            foreach (var school in year.Value)
            foreach (var filename in school.Value)
            {
                var yearKey = year.Key.ToString();
                var schoolKey = school.Key.ToString();
                var path = Path.Join(outFolder, yearKey, schoolKey, filename.Link);
                var ranking = Parser.ParseJson<Ranking>(path);
                if (ranking != null) rankings.Add(ranking);
            }

            return new RankingsSet
            {
                LastUpdate = mainJson.LastUpdate,
                Rankings = rankings
            };
        }
        catch
        {
            // ignored
        }

        return null;
    }
}
