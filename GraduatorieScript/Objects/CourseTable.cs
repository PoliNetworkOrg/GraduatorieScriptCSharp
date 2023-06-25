using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTable : MeritTable
{
    public string? Location;
    public List<string>? Sections;
    public string? Title;
}