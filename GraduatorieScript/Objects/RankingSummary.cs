using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingSummary
{
    public int? HowManyCanEnroll;
    public int? HowManyStudents;
    public Dictionary<int, int>? ResultsSummarized; //key=score, value=howManyGotThatScore
}