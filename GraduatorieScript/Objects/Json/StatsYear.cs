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

    public int GetHashWithoutLastUpdate()
    {
        int i = NumStudents ?? 0;
        foreach (var variable in this.Schools)
        {
            var variableKey = (int)variable.Key;
            var i2 = variableKey ^ variable.Value.GetHashWithoutLastUpdate();
            i ^= i2;
        }

        return i;
    }
}