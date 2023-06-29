using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class SingleCourseJson
{
    public string? Link;
    public string? Name;

    public int GetHashWithoutLastUpdate()
    {
        var hashWithoutLastUpdate = Link?.GetHashCode() ?? 0;
        var hashCode = Name?.GetHashCode() ?? 0;
        return hashWithoutLastUpdate ^ hashCode;
    }
}