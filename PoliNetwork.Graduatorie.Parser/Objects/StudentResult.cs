﻿#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Utils;

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
        var r = new List<int?>
        {
            "StudentResult".GetHashCode(),
            BirthDate?.GetHashCode() ?? "BirthDate".GetHashCode(),
            EnrollType?.GetHashWithoutLastUpdate() ?? "EnrollType".GetHashCode(),
            EnglishCorrectAnswers?.GetHashCode() ?? "EnglishCorrectAnswers".GetHashCode(),
            Id?.GetHashCode() ?? "Id".GetHashCode(),
            PositionAbsolute?.GetHashCode() ?? "PositionAbsolute".GetHashCode(),
            PositionCourse?.GetHashCode() ?? "PositionCourse".GetHashCode(),
            Result?.GetHashCode() ?? "Result".GetHashCode()
        };
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

        return Hashing.GetHashFromListHash(r);
    }
}