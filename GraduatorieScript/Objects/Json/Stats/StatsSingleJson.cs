using GraduatorieScript.Objects.RankingNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSingleCourseJson
{
    public SingleCourseJson? SingleCourseJson;
    public RankingSummary? Stats;

    public static StatsSingleCourseJson From(Ranking.Ranking ranking)
    {
        return new StatsSingleCourseJson
        {
            SingleCourseJson = ranking.ToSingleCourseJson(),
            Stats = ranking.RankingSummary
        };
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = 0;
        i ^= SingleCourseJson?.GetHashWithoutLastUpdate() ?? 0;
        i ^= Stats?.GetHashWithoutLastUpdate() ?? 0;

        return i;
    }
}