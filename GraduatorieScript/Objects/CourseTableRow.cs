using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTableRow
{
    public DateOnly birthDate;
    public bool canEnroll;
    public int? englishCorrectAnswers;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public int position;
    public decimal result;
    public Dictionary<string, decimal>? sectionsResults;
}