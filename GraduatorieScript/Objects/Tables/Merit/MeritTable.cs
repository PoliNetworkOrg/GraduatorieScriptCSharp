using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables.Merit;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MeritTable
{
    public List<string>? Headers;
    public List<StudentResult>? Rows;

    public int GetHashWithoutLastUpdate()
    {
        var i = 0;
        if (Headers != null)
            foreach (var variable in Headers)
                i ^= variable.GetHashCode();

        if (Rows != null)
            foreach (var variable in Rows)
                i ^= variable.GetHashWithoutLastUpdate();
        return i;
    }
}