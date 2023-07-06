using GraduatorieScript.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class SingleCourseJson
{
    public string? BasePath;
    public string? Link;
    public string? Location;
    public string? Name;
    public SchoolEnum? School;
    public int? Year;

    public int GetHashWithoutLastUpdate()
    {
        var hashWithoutLastUpdate = Link?.GetHashCode() ?? "Link".GetHashCode();
        var hashCode = Name?.GetHashCode() ?? "Name".GetHashCode();
        var basePathInt = BasePath?.GetHashCode() ?? "BasePath".GetHashCode();
        var yearInt = Year?.GetHashCode() ?? "Year".GetHashCode();
        var schoolInt = School?.GetHashCode() ?? "School".GetHashCode();
        var code = "SingleCourseJson".GetHashCode();
        return hashWithoutLastUpdate ^ hashCode ^ basePathInt ^ yearInt ^ schoolInt ^ code;
    }
}