using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StudentResult
{
    private DateOnly birthDate;
    private bool canEnroll;
    private string? canEnrollInto;
    private int englishCorrectAnswers;
    private string? id;
    private Dictionary<string, bool>? ofa; // maybe change it
    private Dictionary<string, decimal>? partialResults;
    private int positionMeritAbsolute;
    private int positionMeritInCourse;
    private decimal result;
}
