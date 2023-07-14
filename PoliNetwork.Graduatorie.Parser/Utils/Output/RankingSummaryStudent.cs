using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingSummaryStudent
{
    public string? Phase;
    public SchoolEnum? School;
    public int? Year;
    public RankingUrl? Url;
    public string? Course;

    public override bool Equals(object? obj)
    {
        if (obj is not RankingSummaryStudent rankingSummaryStudent) return false;
        var equals = (Url == null && rankingSummaryStudent.Url == null )  || (Url?.Equals(rankingSummaryStudent.Url) ?? false);
        return Phase == rankingSummaryStudent.Phase && School == rankingSummaryStudent.School &&
               Year == rankingSummaryStudent.Year && equals && Course == rankingSummaryStudent.Course;
    }
}