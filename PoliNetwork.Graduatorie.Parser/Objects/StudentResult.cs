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

    public List<int?> GetHashWithoutLastUpdate()
    {
        List<int?> r = new List<int?>();
        r.Add("StudentResult".GetHashCode());
        r.Add(BirthDate?.GetHashCode() ?? "BirthDate".GetHashCode());
        r.Add(EnrollType?.GetHashWithoutLastUpdate() ?? "EnrollType".GetHashCode());
        r.Add(EnglishCorrectAnswers?.GetHashCode() ?? "EnglishCorrectAnswers".GetHashCode());
        r.Add(Id?.GetHashCode() ?? "Id".GetHashCode());
        r.Add(PositionAbsolute?.GetHashCode() ?? "PositionAbsolute".GetHashCode());
        r.Add(PositionCourse?.GetHashCode() ?? "PositionCourse".GetHashCode());
        r.Add(Result?.GetHashCode() ?? "Result".GetHashCode());
        if (Ofa == null)
            r.Add("OfaEmpty".GetHashCode());
        else
            r.Add(Ofa.Aggregate("OfaFull".GetHashCode(),
                (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode()));

        if (SectionsResults == null)
            r.Add("SectionsResultsEmpty".GetHashCode());
        else
            r.Add(SectionsResults.Aggregate("SectionsResultsFull".GetHashCode(),
                (current, variable) => current ^ variable.Key.GetHashCode() ^ variable.Value.GetHashCode()));

        return r;
    }
}