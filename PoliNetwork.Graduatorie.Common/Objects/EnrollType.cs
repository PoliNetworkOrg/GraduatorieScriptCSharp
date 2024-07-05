#region

using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class EnrollType
{
    public bool CanEnroll;
    public string? Course;
    public string? Type;

    public int GetHashWithoutLastUpdate()
    {
        var i = "EnrollTypeNotNull".GetHashCode();
        i ^= Course?.GetHashCode() ?? "Course".GetHashCode();
        i ^= Type?.GetHashCode() ?? "Type".GetHashCode();

        return i;
    }
    
    public static EnrollType From(string? rowCanEnrollInto, bool rowCanEnroll)
    {
        if (rowCanEnroll == false)
            return new EnrollType { CanEnroll = false, Course = null, Type = null };

        if (string.IsNullOrEmpty(rowCanEnrollInto))
            return new EnrollType { CanEnroll = true, Course = null, Type = null };

        string[] tester = { "assegnato", "prenotato" };
        const string sep = " - ";
        if (!rowCanEnrollInto.Contains(sep) || !tester.Any(t => rowCanEnrollInto.ToLower().Contains(t)))
            return new EnrollType { CanEnroll = true, Course = rowCanEnrollInto, Type = null };

        var s = rowCanEnrollInto.Split(sep).ToList();
        var type = s.FirstOrDefault(x => tester.Any(t => t == x.ToLower()));

        if (type != null) s.Remove(type);

        var course = string.Join(sep, s);
        return new EnrollType { CanEnroll = true, Course = course, Type = type };
    }
}