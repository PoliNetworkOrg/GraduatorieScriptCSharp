#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingSummaryStudent
{
    public readonly string? Course;
    public readonly string? Phase;
    public readonly SchoolEnum? School;
    public readonly RankingUrl? Url;
    public readonly int? Year;

    public RankingSummaryStudent()
    {
    }

    public RankingSummaryStudent(string? course, string? phase, SchoolEnum? school, RankingUrl? url, int? year)
    {
        Course = course;
        Phase = phase;
        School = school;
        Url = url;
        Year = year;
    }

    public RankingSummaryStudent(string? phase, SchoolEnum? school, int? year, RankingUrl? url) : this()
    {
        Phase = phase;
        School = school;
        Year = year;
        Url = url;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RankingSummaryStudent rankingSummaryStudent) return false;
        var equals = (Url == null && rankingSummaryStudent.Url == null) ||
                     (Url?.Equals(rankingSummaryStudent.Url) ?? false);
        return Phase == rankingSummaryStudent.Phase && School == rankingSummaryStudent.School &&
               Year == rankingSummaryStudent.Year && equals && Course == rankingSummaryStudent.Course;
    }

    protected bool Equals(RankingSummaryStudent other)
    {
        return Phase == other.Phase && School == other.School && Year == other.Year && Equals(Url, other.Url) &&
               Course == other.Course;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Phase, School, Year, Url, Course);
    }
}