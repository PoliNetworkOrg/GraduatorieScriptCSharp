using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StatsSingleJson
{
    public SingleCourseJson? SingleCourseJson;
    public RankingSummary? Stats;

    public static StatsSingleJson From(Ranking ranking)
    {
        return new StatsSingleJson
        {
            SingleCourseJson = ranking.ToSingleCourseJson(),
            Stats = ranking.RankingSummary
        };
    }
}
