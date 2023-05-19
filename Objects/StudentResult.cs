using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StudentResult
{
    private string? id;
    private DateOnly birthDate;
    private int positionMeritInDegree;
    private int positionMeritAbsolute;
    private int englishCorrectAnswers;
    private decimal result;
    private Dictionary<string, decimal>? partialResults;
    private bool canEnroll;
    private string? canEnrollInto;
    private Dictionary<string, bool>? ofa;
}