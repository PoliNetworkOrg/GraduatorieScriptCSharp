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
        if (this.Headers != null)
        {
            foreach (var variable in this.Headers)
            {
                i ^= variable.GetHashCode();
            }
        }

        if (this.Rows != null)
        {
            foreach (var variable in this.Rows)
            {
                i ^= variable.GetHashWithoutLastUpdate();
            }
        }
        return i;
    }
}