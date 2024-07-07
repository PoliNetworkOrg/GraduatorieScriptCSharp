#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

// ReSharper disable CanSimplifyDictionaryLookupWithTryAdd

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int NumStudents;

    public static StatsSchool From(IEnumerable<Ranking> pRankings)
    {
        var statsSchool = new StatsSchool();
        var rankings = pRankings.Where(r => r is { Year: not null, School: not null }).ToList();

        statsSchool.NumStudents =
            rankings.Select(x => (x.RankingSummary ?? x.CreateSummary()).HowManyStudents ?? 0).Sum();

        statsSchool.List = rankings
            .SelectMany(r => r.ToStats())
            .DistinctBy(x => new { x.SingleCourseJson.Id, x.SingleCourseJson.Location })
            .OrderBy(x => x.SingleCourseJson.Id)
            .ToList();

        return statsSchool;
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents;
        return List.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
    }
}