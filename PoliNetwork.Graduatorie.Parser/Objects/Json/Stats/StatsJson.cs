#region

using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsJson
{
    private const string StatsFolderName = "stats";

    public DateTime LastUpdate = DateTime.UtcNow;
    public SortedDictionary<int, StatsYear> Stats = new();

    public static StatsJson From(RankingsSet rankingsSet)
    {
        var statsJson = new StatsJson();

        var byYears = rankingsSet.Rankings.GroupBy(r => r.Year);
        foreach (var yearGroup in byYears)
        {
            var statsYear = StatsYear.From(yearGroup.ToList());

            if (statsJson.Stats.ContainsKey(yearGroup.Key)) throw new UnreachableException(); // should be impossible
            statsJson.Stats.Add(yearGroup.Key, statsYear);
        }

        return statsJson;
    }

    public void Write(string outFolder, ArgsConfig argsConfig)
    {
        var statsFolderPath = Path.Join(outFolder, StatsFolderName);
        if (!Directory.Exists(statsFolderPath)) Directory.CreateDirectory(statsFolderPath);

        foreach (var yearStats in Stats.Values) yearStats.Write(statsFolderPath, argsConfig);
    }

    public int GetHashWithoutLastUpdate()
    {
        return Stats.Select(variable => variable.Key ^ variable.Value.GetHashWithoutLastUpdate())
            .Aggregate(0, (current, i2) => current ^ i2);
    }
}