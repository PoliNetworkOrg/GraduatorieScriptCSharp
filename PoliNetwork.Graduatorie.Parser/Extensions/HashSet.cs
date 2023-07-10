using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Extensions;

/// <summary>
///     Extension of Hashset in order to implement some methods like AddRange
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public static class HashSetExtensions
{
    /// <summary>
    ///     Add a list of objects to the hashset
    /// </summary>
    public static void AddRange<T>(this HashSet<T> hashSet, params IEnumerable<T>[] lists)
    {
        foreach (var list in lists)
        foreach (var item in list)
            hashSet.Add(item);
    }
}