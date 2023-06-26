using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StatsJson
{
    public DateTime? LastUpdate;
    public Dictionary<int, StatsYear>? Stats;

    public static void Write(string outFolder, RankingsSet rankingsSet)
    {
        var statsJson = Generate(rankingsSet);
        statsJson.WriteToFile(outFolder);
    }

    private static StatsJson Generate(RankingsSet rankingsSet)
    {
        var statsJson = new StatsJson();
        statsJson.Stats ??= new Dictionary<int, StatsYear>();
        foreach (var variable in rankingsSet.Rankings)
        {
            if (variable.Year == null)
                continue;
            if (!statsJson.Stats.ContainsKey(variable.Year.Value))
            {
                var statsJsonStat = new StatsYear
                {
                    NumStudents = rankingsSet.Rankings.Where(x => x.Year == variable.Year)
                        .Select(x => x.RankingSummary?.HowManyStudents).Sum()
                };
                statsJson.Stats[variable.Year.Value] = statsJsonStat;
            }

            if (variable.School == null)
                continue;
            statsJson.Stats[variable.Year.Value].Dict ??= new Dictionary<SchoolEnum, StatsSchool>();
            var statsSchools = statsJson.Stats[variable.Year.Value].Dict!;
            var containsKey = statsSchools?.ContainsKey(variable.School.Value) ?? false;
            if (!containsKey)
                if (statsSchools != null)
                {
                    var statsSchool = new StatsSchool
                    {
                        NumStudents = rankingsSet.Rankings
                            .Where(x => x.Year == variable.Year && x.School == variable.School)
                            .Select(x => x.RankingSummary?.HowManyStudents).Sum()
                    };
                    statsSchools[variable.School.Value] = statsSchool;
                }

            statsSchools?[variable.School.Value].List?.Add(variable.ToStats());
        }

        return statsJson;
    }

    private void WriteToFile(string outFolder)
    {
        var mainJsonPath = Path.Join(outFolder, Constants.StatsJsonFilename);
        var mainJsonString = JsonConvert.SerializeObject(this);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }
}