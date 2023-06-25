using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingSummary
{
    public int? howManyEnrolled;
    public int? howManyStudents;
}