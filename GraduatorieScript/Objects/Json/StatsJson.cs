using GraduatorieScript.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsJson
{
    public DateTime LastUpdate = DateTime.Now;
    public Dictionary<int, StatsYear> Stats = new();

    public static void Write(string outFolder, RankingsSet rankingsSet)
    {
        var statsJson = Generate(rankingsSet);
        statsJson.WriteToFile(outFolder);
    }

    private static StatsJson Generate(RankingsSet rankingsSet)
    {
        var statsJson = new StatsJson();
        foreach (var ranking in rankingsSet.Rankings)
        {
            if (ranking.Year == null) continue;
            if (!statsJson.Stats.ContainsKey(ranking.Year.Value))
            {
                var statsJsonStat = new StatsYear
                {
                    NumStudents = rankingsSet.Rankings.Where(x => x.Year == ranking.Year)
                        .Select(x => x.RankingSummary?.HowManyStudents).Sum()
                };
                statsJson.Stats[ranking.Year.Value] = statsJsonStat;
            }

            if (ranking.School == null) continue;
            var schools = statsJson.Stats[ranking.Year.Value].Schools;
            if (!schools.ContainsKey(ranking.School.Value))
            {
                var statsSchool = new StatsSchool
                {
                    NumStudents = rankingsSet.Rankings
                        .Where(x => x.Year == ranking.Year && x.School == ranking.School)
                        .Select(x => x.RankingSummary?.HowManyStudents).Sum()
                };
                schools[ranking.School.Value] = statsSchool;
            }

            schools[ranking.School.Value].List.Add(ranking.ToStats());
        }

        return statsJson;
    }

    private void WriteToFile(string outFolder)
    {
        var mainJsonPath = Path.Join(outFolder, Constants.StatsJsonFilename);
        var mainJsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }
}
