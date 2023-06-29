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
        i ^= this.BirthDate.GetHashCode();
        i ^= this.CanEnroll.GetHashCode();
        i ^= this.CanEnrollInto?.GetHashCode() ?? "CanEnrollInto".GetHashCode();
        i ^= this.EnglishCorrectAnswers.GetHashCode();
        i ^= this.Id?.GetHashCode() ?? "Id".GetHashCode();
        i ^= this.PositionAbsolute.GetHashCode();
        i ^= this.PositionCourse.GetHashCode();
        i ^= this.Result.GetHashCode();
        if (this.Ofa != null)
        {
            foreach (var variable in this.Ofa)
            {
                i ^= variable.Key.GetHashCode() ^ variable.Value.GetHashCode();
            }
        }

        if (this.SectionsResults != null)
        {
            foreach (var variable in this.SectionsResults)
            {
                i ^= variable.Key.GetHashCode() ^ variable.Value.GetHashCode();
            }
        }
        return i;
    }
}