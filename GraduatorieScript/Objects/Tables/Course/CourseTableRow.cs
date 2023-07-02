using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTableRow
{
    public DateOnly BirthDate;
    public bool CanEnroll;
    public int? EnglishCorrectAnswers;
    public string? Id;
    public Dictionary<string, bool>? Ofa; // maybe change it
    public int Position;
    public decimal Result;
    public Dictionary<string, decimal>? SectionsResults;
}