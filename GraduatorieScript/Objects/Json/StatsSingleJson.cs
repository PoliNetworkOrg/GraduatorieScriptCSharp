using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSingleCourseJson
{
    public SingleCourseJson? SingleCourseJson;
    public RankingSummary? Stats;

    public static StatsSingleCourseJson From(Ranking ranking)
    {
        return new StatsSingleCourseJson
        {
            SingleCourseJson = ranking.ToSingleCourseJson(),
            Stats = ranking.RankingSummary
        };
    }
}
