#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public abstract class Table : Table<List<string>>
{
}

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Table<T>
{
    public string? CourseLocation;
    public string? CourseTitle;
    public List<T> Data = new();
    public List<string> Headers = new();
    public List<string>? Sections;

    public static Table<T> Create(List<string> headers, List<string>? sections, List<T> data, string? courseTitle,
        string? courseLocation)
    {
        return new Table<T>
        {
            Headers = headers,
            Sections = sections,
            Data = data,
            CourseTitle = courseTitle,
            CourseLocation = courseLocation
        };
    }

    public static string? GetFieldByIndex(List<string> row, int index)
    {
        if (index == -1 || index >= row.Count) return null;
        var fieldByIndex = row[index];
        return string.IsNullOrEmpty(fieldByIndex) ? null : fieldByIndex;
    }

    public SortedDictionary<string, int>? GetSectionsIndex()
    {
        if (Sections is null) return null;
        var dict = new SortedDictionary<string, int>();
        foreach (var section in Sections)
        {
            var index = Headers.FindIndex(h => h == section);
            if (index != -1) dict.Add(section, index);
        }

        return dict;
    }
}