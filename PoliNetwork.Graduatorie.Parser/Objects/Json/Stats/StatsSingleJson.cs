using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSingleCourseJson
{
    public SingleCourseJson? SingleCourseJson;
    public RankingSummary? Stats;

    public static List<StatsSingleCourseJson> From(Ranking ranking)
    {
        var singleCourseJsons = ranking.ToSingleCourseJson();
        return singleCourseJsons.Select(variable => new StatsSingleCourseJson
            { SingleCourseJson = variable, Stats = ranking.RankingSummary }).ToList();
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = "StatsSingleCourseJson".GetHashCode();
        i ^= SingleCourseJson?.GetHashWithoutLastUpdate() ?? "SingleCourseJson".GetHashCode();
        i ^= Stats?.GetHashWithoutLastUpdate() ?? "Stats".GetHashCode();

        return i;
    }
}