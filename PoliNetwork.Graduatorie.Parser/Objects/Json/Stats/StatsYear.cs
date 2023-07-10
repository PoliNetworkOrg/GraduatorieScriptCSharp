using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsYear
{
    public int? NumStudents;
    public Dictionary<SchoolEnum, StatsSchool> Schools = new();

    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents ?? "NumStudents".GetHashCode();

        var enumerable = from variable in Schools
            let variableKey = (int)variable.Key
            select variableKey ^ variable.Value.GetHashWithoutLastUpdate();
        return enumerable.Aggregate(i, (current, i2) => current ^ i2);
    }
}