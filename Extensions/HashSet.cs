using Newtonsoft.Json;

namespace GraduatorieScript.Extensions;

/// <summary>
///     Extension of Hashset in order to implement some methods like AddRange
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public static class HashSetExtensions
{
    /// <summary>
    ///     Add a list of objects to the hashset
    /// </summary>
    /// <param name="list">list to add</param>
    public static void AddRange<T>(this HashSet<T> hashSet, params IEnumerable<T>[] lists)
    {
        foreach (var list in lists)
            foreach (var item in list) hashSet.Add(item);
    }
}
