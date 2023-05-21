namespace GraduatorieScript.Objects;

/// <summary>
/// Extension of Hashset in order to implement some methods like AddRange
/// </summary>
/// <typeparam name="T"></typeparam>
public class HashSetExtended<T> : HashSet<T>
{
    /// <summary>
    /// Add a list of objects to the hashset
    /// </summary>
    /// <param name="list">list to add</param>
    public void AddRange(HashSet<T> list)
    {
        foreach (var item in list)
        {
            this.Add(item);
        }
    }
}