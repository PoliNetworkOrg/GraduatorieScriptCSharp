#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class SingleCourseJson: IComparable<SingleCourseJson>
{
    public string? BasePath;
    public string? Link;
    public string? Location;
    public string? Id;
    public RankingOrder? RankingOrder;
    public SchoolEnum? School;
    public int? Year;

    public static SingleCourseJson From(Ranking ranking, CourseTable? course)
    {
        var basePath = $"{ranking.School}/{ranking.Year}/"; // "Ingegneria/2023"
        return new SingleCourseJson
        {
            Link = ranking.GetFilename(),
            Id = ranking.GetId(),
            BasePath = basePath,
            Year = ranking.Year,
            School = ranking.School,
            Location = course?.Location,
            RankingOrder = ranking.RankingOrder
        };
    }

    public int GetHashWithoutLastUpdate()
    {
        var hashWithoutLastUpdate = Link?.GetHashCode() ?? "Link".GetHashCode();
        var hashCode = Id?.GetHashCode() ?? "Id".GetHashCode();
        var basePathInt = BasePath?.GetHashCode() ?? "BasePath".GetHashCode();
        var yearInt = Year?.GetHashCode() ?? "Year".GetHashCode();
        var schoolInt = School?.GetHashCode() ?? "School".GetHashCode();
        var code = "SingleCourseJson".GetHashCode();
        return hashWithoutLastUpdate ^ hashCode ^ basePathInt ^ yearInt ^ schoolInt ^ code;
    }

    public int CompareTo(SingleCourseJson? singleCourseJson)
    {
        if (singleCourseJson == null) return 1;
        
        if (Year != singleCourseJson.Year)
            return (Year ?? -1) < (singleCourseJson.Year ?? -1) ? -1 : 1;

        if (School != singleCourseJson.School) return School < singleCourseJson.School ? -1 : 1;

        if (BasePath != singleCourseJson.BasePath)
            return string.Compare(BasePath ?? "", singleCourseJson.BasePath ?? "", StringComparison.InvariantCulture);

        if (Link != singleCourseJson.Link)
            return string.Compare(Link ?? "", singleCourseJson.Link ?? "", StringComparison.InvariantCulture);

        if (Location != singleCourseJson.Location)
            return string.Compare(Location ?? "", singleCourseJson.Location ?? "", StringComparison.InvariantCulture);

        if (Id != singleCourseJson.Id)
            return string.Compare(Id ?? "", singleCourseJson.Id ?? "", StringComparison.InvariantCulture);

        return 0;
    }

    public bool Is(CourseTable courseTable)
    {
        return (RankingOrder?.Phase ?? "") == courseTable.Title;
    }
}