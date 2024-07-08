#region

using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

using SchoolsDict = SortedDictionary<SchoolEnum, StatsSchool>;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsYear
{
    public int NumStudents;
    public SchoolsDict Schools = new();
    public int Year;

    public static StatsYear From(List<Ranking> rankings)
    {
        if (rankings.Count == 0) return new StatsYear();
        var statsYear = new StatsYear
        {
            Year = rankings.First().Year, // just hilarious
            NumStudents = rankings.Select(r => r.RankingSummary.HowManyStudents ?? 0).Sum() // this ?? is crazy
        };

        var bySchool = rankings.GroupBy(r => r.School);
        foreach (var schoolGroup in bySchool)
        {
            var statsSchool = StatsSchool.From(schoolGroup.ToList());

            if (statsYear.Schools.ContainsKey(schoolGroup.Key))
                throw new UnreachableException(); // should be impossible, right?
            statsYear.Schools.Add(schoolGroup.Key, statsSchool);
        }

        return statsYear;
    }

    public void Write(string statsFolderPath, ArgsConfig argsConfig)
    {
        var fullJsonPath = Path.Join(statsFolderPath, $"{Year}.json");
        if (ExitIfThereIsntAnUpdate(fullJsonPath) && !argsConfig.ForceReparsing) return;

        var jsonString = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        File.WriteAllText(fullJsonPath, jsonString);
    }

    private bool ExitIfThereIsntAnUpdate(string fullJsonPath)
    {
        try
        {
            if (!File.Exists(fullJsonPath)) return false;

            var saved = File.ReadAllText(fullJsonPath);
            var savedStats = JsonConvert.DeserializeObject<StatsYear>(saved, Culture.JsonSerializerSettings);

            if (string.IsNullOrEmpty(saved) || savedStats == null) return false;

            var savedHash = savedStats.GetHashWithoutLastUpdate();
            var hash = GetHashWithoutLastUpdate();
            return savedHash == hash;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return false;
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents;

        var enumerable = from variable in Schools
            let variableKey = (int)variable.Key
            select variableKey ^ variable.Value.GetHashWithoutLastUpdate();
        return enumerable.Aggregate(i, (current, i2) => current ^ i2);
    }
}