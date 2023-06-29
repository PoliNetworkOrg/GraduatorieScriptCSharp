using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int? NumStudents;

    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents ?? 0;
        return List.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
    }
}