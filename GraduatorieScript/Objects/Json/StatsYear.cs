using GraduatorieScript.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsYear
{
    public int? NumStudents;
    public Dictionary<SchoolEnum, StatsSchool> Schools = new();
}