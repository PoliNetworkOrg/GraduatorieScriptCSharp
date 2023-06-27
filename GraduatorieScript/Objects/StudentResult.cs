using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
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