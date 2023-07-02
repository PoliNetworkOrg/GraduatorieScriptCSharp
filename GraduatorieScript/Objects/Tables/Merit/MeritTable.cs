using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables.Merit;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MeritTable
{
    public List<string>? Headers;
    public string? Path;
    public List<StudentResult>? Rows;
    public int? Year;

    public int GetHashWithoutLastUpdate()
    {
        var i = 0;
        if (Headers != null) 
            i = Headers.Aggregate(i, (current, variable) => current ^ variable.GetHashCode());

        if (Rows != null) 
            i = Rows.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());

        i ^= Year?.GetHashCode() ?? "Year".GetHashCode();
        i ^= Path?.GetHashCode() ?? "Path".GetHashCode();
        return i;
    }
}