#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsJson
{
    private const string PathStats = "stats";

    public DateTime LastUpdate = DateTime.UtcNow;
    public SortedDictionary<int, StatsYear> Stats = new();

    public static void Write(string outFolder, RankingsSet rankingsSet, ArgsConfig argsConfig)
    {
        var statsJson = From(rankingsSet);
        foreach (var yearDict in statsJson.Stats) WriteToFileYear(outFolder, yearDict, argsConfig);
    }

    private static StatsJson From(RankingsSet rankingsSet)
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

        var statsSingleCourseJsons = ranking.ToStats().DistinctBy(x => new
        {
            x.SingleCourseJson?.Link, x.SingleCourseJson?.Location
        });
        foreach (var variable in statsSingleCourseJsons) schools[ranking.School.Value].List.Add(variable);
    }

    private static void WriteToFileYear(string outFolder, KeyValuePair<int, StatsYear> yearDict, ArgsConfig argsConfig)
    {
        var statsPath = Path.Join(outFolder, PathStats);
        if (!Directory.Exists(statsPath)) Directory.CreateDirectory(statsPath);

        var jsonPath = Path.Join(statsPath, yearDict.Key + ".json");
        if (ExitIfThereIsntAnUpdate(jsonPath, yearDict.Value) && !argsConfig.ForceReparsing) return;

        var jsonString = JsonConvert.SerializeObject(yearDict.Value, Culture.JsonSerializerSettings);
        File.WriteAllText(jsonPath, jsonString);
    }

    private static bool ExitIfThereIsntAnUpdate(string jsonPath, StatsYear variableValue)
    {
        try
        {
            if (!File.Exists(jsonPath)) return false;

            var read = File.ReadAllText(jsonPath);
            var jsonRead = JsonConvert.DeserializeObject<StatsYear>(read, Culture.JsonSerializerSettings);
            var hashRead = jsonRead?.GetHashWithoutLastUpdate();
            var hashThis = variableValue.GetHashWithoutLastUpdate();

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