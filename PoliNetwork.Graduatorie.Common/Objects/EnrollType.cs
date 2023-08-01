#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class EnrollType
{
    public bool? CanEnroll;
    public string? Course;
    public string? Type;

    public int GetHashWithoutLastUpdate()
    {
        var i = "EnrollTypeNotNull".GetHashCode();
        i ^= Course?.GetHashCode() ?? "Course".GetHashCode();
        i ^= Type?.GetHashCode() ?? "Type".GetHashCode();
        i ^= CanEnroll?.GetHashCode() ?? "CanEnroll".GetHashCode();

        return i;
    }
}