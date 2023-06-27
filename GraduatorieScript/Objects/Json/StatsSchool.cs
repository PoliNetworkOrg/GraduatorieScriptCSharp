using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int? NumStudents;
}
