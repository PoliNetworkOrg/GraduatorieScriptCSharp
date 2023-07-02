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

    public int GetHashWithoutLastUpdate()
    {
        var i = 0;
        i ^= BirthDate.GetHashCode();
        i ^= CanEnroll.GetHashCode();
        i ^= CanEnrollInto?.GetHashCode() ?? "CanEnrollInto".GetHashCode();
        i ^= EnglishCorrectAnswers.GetHashCode();
        i ^= Id?.GetHashCode() ?? "Id".GetHashCode();
        i ^= PositionAbsolute.GetHashCode();
        i ^= PositionCourse.GetHashCode();
        i ^= Result.GetHashCode();
        if (Ofa != null) 
            i = Ofa.Aggregate(i, (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode());

        if (SectionsResults != null) 
            i = SectionsResults.Aggregate(i, (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode());
        
        return i;
    }
}