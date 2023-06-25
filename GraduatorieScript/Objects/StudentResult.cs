using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StudentResult
{
    public DateOnly? birthDate;
    public bool canEnroll;
    public string? canEnrollInto;
    public int? englishCorrectAnswers;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public Dictionary<string, decimal>? sectionsResults;
    public int? positionAbsolute;
    public int? positionCourse;
    public decimal result;
}
