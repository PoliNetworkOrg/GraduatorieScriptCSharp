using GraduatorieScript.Objects.Tables.Course;
using GraduatorieScript.Objects.Tables.Merit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTable : MeritTable
{
    public string? Location;
    public List<string>? Sections;
    public string? Title;

    public CourseTableStats GetStats()
    {
        return CourseTableStats.From(this);
    }
}