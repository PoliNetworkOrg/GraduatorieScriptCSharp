using GraduatorieScript.Data.Constants;
using GraduatorieScript.Objects.RankingNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Stats;

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
        foreach (var ranking in rankingsSet.Rankings) GenerateSingleRanking(rankingsSet, ranking, statsJson);

        foreach (var year in statsJson.Stats.Keys)
        foreach (var school in statsJson.Stats[year].Schools.Keys)
        {
            var statsSingleCourseJsons =
                statsJson.Stats[year].Schools[school].List.OrderBy(x => x.SingleCourseJson?.Link);
            statsJson.Stats[year].Schools[school].List = statsSingleCourseJsons.ToList();
        }

        return statsJson;
    }

    private static void GenerateSingleRanking(RankingsSet rankingsSet, Ranking ranking, StatsJson statsJson)
    {
        if (ranking.Year == null) return;
        if (!statsJson.Stats.ContainsKey(ranking.Year.Value))
        {
            var statsJsonStat = new StatsYear
            {
                NumStudents = rankingsSet.Rankings.Where(x => x.Year == ranking.Year)
                    .Select(x => x.RankingSummary?.HowManyStudents).Sum()
            };
            statsJson.Stats[ranking.Year.Value] = statsJsonStat;
        }

        if (ranking.School == null) return;
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

    private void WriteToFile(string outFolder)
    {
        var jsonPath = Path.Join(outFolder, Constants.StatsJsonFilename);

        if (ExitIfThereIsntAnUpdate(jsonPath)) return;

        var jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(jsonPath, jsonString);
    }

    private bool ExitIfThereIsntAnUpdate(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath)) return false;

            var read = File.ReadAllText(jsonPath);
            var jsonRead = JsonConvert.DeserializeObject<StatsJson>(read);
            var hashRead = jsonRead?.GetHashWithoutLastUpdate();
            var hashThis = GetHashWithoutLastUpdate();

            return hashRead == hashThis;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return false;
    }

    public int GetHashWithoutLastUpdate()
    {
        return Stats.Select(variable => variable.Key ^ variable.Value.GetHashWithoutLastUpdate())
            .Aggregate(0, (current, i2) => current ^ i2);
    }
}
