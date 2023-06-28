using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StudentResult
{
    public DateOnly? BirthDate;
    public bool CanEnroll;
    public string? CanEnrollInto;
    public int? EnglishCorrectAnswers;
    public string? Id;
    public Dictionary<string, bool>? Ofa; // maybe change it
    public int? PositionAbsolute;
    public int? PositionCourse;
    public decimal Result;
    public Dictionary<string, decimal>? SectionsResults;
}