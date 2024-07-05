#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;
using PoliNetwork.Graduatorie.Parser.Utils.Output;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTable : MeritTable
{
    public string? Location;
    public List<string>? Sections;
    public string? Title;

    public CourseTableStats GetStats()
    {
        return CourseTableStats.From(this);
    }

    public RankingSummaryStudent GetRankingSummaryStudent(Ranking ranking)
    {
        return new RankingSummaryStudent(Title, ranking.RankingOrder?.Phase, ranking.School,
            ranking.Url, ranking.Year);
    }

    /// <summary>
    /// Get the course location if present, otherwise get the placeholder (constant).
    /// Useful for index purposes.
    /// </summary>
    /// <returns>A string with the location or the placeholder</returns>
    public string GetFixedLocation()
    {
        // fixedLocation
        // esempio: Urbanistica 2022 ha un solo corso senza location, ma anche quello
        // deve comparire nella lista
        // fix: se un corso non ha location, si inserisce un valore 0
        if (string.IsNullOrEmpty(Location)) return Constants.LocationPlaceholder;
        return Location;
    }
}