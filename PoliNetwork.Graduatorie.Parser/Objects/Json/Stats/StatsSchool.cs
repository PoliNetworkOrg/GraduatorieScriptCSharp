#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int NumStudents;
    
    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents;
        return List.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
    }
}