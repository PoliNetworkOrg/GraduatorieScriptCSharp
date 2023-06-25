using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Utils.Transformer;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MainJson
{
    public DateTime? LastUpdate;
    public Dictionary<int, Dictionary<SchoolEnum, IEnumerable<string>>> Years = new();

    public static void Write(string docsFolder, RankingsSet set)
    {
        var mainJson = new MainJson();
        var outFolder = Path.Join(docsFolder, Constants.OutputFolder);
        mainJson.LastUpdate = set.LastUpdate;
        // group rankings by year
        var byYears = set.Rankings.GroupBy(r => r.year);
        foreach (var yearGroup in byYears)
        {
            if (yearGroup.Key is null) continue;
            var year = yearGroup.Key.Value;
            var bySchool = yearGroup.GroupBy(r => r.school);

            var yearDict = new Dictionary<SchoolEnum, IEnumerable<string>>();

            foreach (var schoolGroup in bySchool)
            {
                if (schoolGroup.Key is null) continue;
                var school = schoolGroup.Key.Value;
                var folder = Path.Join(outFolder, year.ToString(), school.ToString());
                Directory.CreateDirectory(folder);

                foreach (var ranking in schoolGroup)
                {
                    if (ranking is null) continue;
                    var path = Path.Join(folder, ranking.ConvertPhaseToFilename());
                    var rankingJsonString = JsonConvert.SerializeObject(ranking);
                    File.WriteAllText(path, rankingJsonString);
                }

                var filenames = schoolGroup.Select(ranking => ranking.ConvertPhaseToFilename());
                yearDict.Add(school, filenames);
            }

            mainJson.Years.Add(year, yearDict);
        }

        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        var mainJsonString = JsonConvert.SerializeObject(mainJson);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }

    public static RankingsSet? Parse(string docsFolder)
    {
        var outFolder = Path.Join(docsFolder, Constants.OutputFolder);
        var mainJsonPath = Path.Join(outFolder, Constants.MainJsonFilename);
        var mainJson = Parser.ParseJson<MainJson>(mainJsonPath);
        if (mainJson is null) return null;

        List<Ranking> rankings = new();
        foreach (var year in mainJson.Years)
        foreach (var school in year.Value)
        foreach (var filename in school.Value)
        {
            var yearKey = year.Key.ToString();
            var schoolKey = school.Key.ToString();
            var path = Path.Join(outFolder, yearKey, schoolKey, filename);
            var ranking = Parser.ParseJson<Ranking>(path);
            if (ranking is Ranking) rankings.Add(ranking);
        }

        return new RankingsSet
        {
            LastUpdate = mainJson.LastUpdate,
            Rankings = rankings
        };
    }
}