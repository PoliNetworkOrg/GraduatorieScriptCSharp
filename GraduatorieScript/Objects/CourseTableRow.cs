using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
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