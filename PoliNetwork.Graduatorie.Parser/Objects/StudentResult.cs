#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StudentResult
{
    public DateOnly? BirthDate;
    public int? EnglishCorrectAnswers;
    public EnrollType? EnrollType;
    public string? Id;
    public SortedDictionary<string, bool>? Ofa; // maybe change it
    public int? PositionAbsolute;
    public int? PositionCourse;
    public decimal? Result;
    public SortedDictionary<string, decimal>? SectionsResults;

    public int GetHashWithoutLastUpdate()
    {
        var i = "StudentResult".GetHashCode();
        i ^= BirthDate?.GetHashCode() ?? "BirthDate".GetHashCode();
        i ^= EnrollType?.GetHashWithoutLastUpdate() ?? "EnrollType".GetHashCode();
        i ^= EnglishCorrectAnswers?.GetHashCode() ?? "EnglishCorrectAnswers".GetHashCode();
        i ^= Id?.GetHashCode() ?? "Id".GetHashCode();
        i ^= PositionAbsolute?.GetHashCode() ?? "PositionAbsolute".GetHashCode();
        i ^= PositionCourse?.GetHashCode() ?? "PositionCourse".GetHashCode();
        i ^= Result?.GetHashCode() ?? "Result".GetHashCode();
        i ^= EnrollType?.GetHashCode() ?? "EnrollType".GetHashCode();
        if (Ofa == null)
            i ^= "Ofa".GetHashCode();
        else
            i = Ofa.Aggregate(i,
                (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode());

        if (SectionsResults == null)
            i ^= "SectionsResults".GetHashCode();
        else
            i = SectionsResults.Aggregate(i,
                (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode());

        return i;
    }
}