using GraduatorieScript.Enums;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StatsYear
{
    public Dictionary<SchoolEnum, StatsSchool>? Dict;
    public int? NumStudents;

    public StatsYear()
    {
        Dict = new Dictionary<SchoolEnum, StatsSchool>();
    }
}