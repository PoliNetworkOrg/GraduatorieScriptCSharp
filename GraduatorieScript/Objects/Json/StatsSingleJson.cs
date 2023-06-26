using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
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
