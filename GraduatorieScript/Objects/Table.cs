namespace GraduatorieScript.Utils.Transformer;

public abstract class Table : Table<List<string>> { }

public class Table<T>
{
    public List<string> Headers = new();
    public List<string>? Sections;
    public List<T> Data = new();
    public string? CourseTitle;
    public string? CourseLocation;

    public Table() { }

    public static Table<T> Create(List<string> headers, List<string>? sections, List<T> data, string? courseTitle, string? courseLocation)
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
        return row[index];
    }

    public Dictionary<string, int>? GetSectionsIndex()
    {
        if (Sections is null) return null;
        var dict = new Dictionary<string, int>();
        foreach (var section in Sections)
        {
            var index = Headers.FindIndex(h => h == section);
            if (index != -1) dict.Add(section, index);
        }
        return dict;
    }
}