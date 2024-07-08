#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingSummaryStudent : IEquatable<RankingSummaryStudent>, IComparable<RankingSummaryStudent>
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

    public bool Equals(RankingSummaryStudent? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Course == other.Course && Phase == other.Phase && School == other.School && Equals(Url, other.Url) &&
               Year == other.Year;
    }

    public int CompareTo(RankingSummaryStudent? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        
        var i = (Year ?? 0) - (other.Year ?? 0);
        if (i != 0) return i < 0 ? -1 : 1;

        i = string.CompareOrdinal(Course ?? "", other.Course ?? "");
        if (i != 0) return i < 0 ? -1 : 1;

        i = string.CompareOrdinal(Phase ?? "", other.Phase ?? "");
        if (i != 0) return i < 0 ? -1 : 1;

        i = (int)(School ?? SchoolEnum.Unknown) - (int)(other.School ?? SchoolEnum.Unknown);
        if (i != 0) return i < 0 ? -1 : 1;

        i = Url?.CompareTo(other.Url) ?? 0;
        if (i != 0) return i < 0 ? -1 : 1;


        return i;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RankingSummaryStudent rankingSummaryStudent) return false;
        var equals = (Url == null && rankingSummaryStudent.Url == null) ||
                     (Url?.Equals(rankingSummaryStudent.Url) ?? false);
        return Phase == rankingSummaryStudent.Phase && School == rankingSummaryStudent.School &&
               Year == rankingSummaryStudent.Year && equals && Course == rankingSummaryStudent.Course;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Course, Phase, School, Url, Year);
    }
}