#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSingleCourseJson
{
    public SingleCourseJson SingleCourseJson;
    public RankingSummary Stats;

    public StatsSingleCourseJson(SingleCourseJson singleCourseJson, RankingSummary stats)
    {
        SingleCourseJson = singleCourseJson;
        Stats = stats;
    }

    public static List<StatsSingleCourseJson> From(Ranking ranking)
    {
        var singleCourseJsons = ranking.ToSingleCourseJson();
        if (ranking.RankingSummary != null) ranking.RankingSummary = ranking.CreateSummary();
        return singleCourseJsons.Select(scj => new StatsSingleCourseJson(scj, ranking.RankingSummary!)).ToList();
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = "StatsSingleCourseJson".GetHashCode();
        i ^= SingleCourseJson.GetHashWithoutLastUpdate();
        i ^= Stats.GetHashWithoutLastUpdate();

        return i;
    }
}