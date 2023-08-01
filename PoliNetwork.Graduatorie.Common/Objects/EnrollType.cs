using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class EnrollType
{
    public string? Course;
    public string? Type;
    public bool? CanEnroll;

    public int GetHashWithoutLastUpdate()
    {
        var i = "EnrollTypeNotNull".GetHashCode();
        i ^= this.Course?.GetHashCode() ?? "Course".GetHashCode();
        i ^= this.Type?.GetHashCode() ?? "Type".GetHashCode();
        i ^= this.CanEnroll?.GetHashCode() ?? "CanEnroll".GetHashCode();

        return i;
    }
}