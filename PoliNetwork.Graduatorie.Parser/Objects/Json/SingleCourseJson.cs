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
public class SingleCourseJson
{
    public string? BasePath;
    public string? Link;
    public string? Location;
    public string? Name;
    public RankingOrder? RankingOrder;
    public SchoolEnum? School;
    public int? Year;

    public int GetHashWithoutLastUpdate()
    {
        var hashWithoutLastUpdate = Link?.GetHashCode() ?? "Link".GetHashCode();
        var hashCode = Name?.GetHashCode() ?? "Name".GetHashCode();
        var basePathInt = BasePath?.GetHashCode() ?? "BasePath".GetHashCode();
        var yearInt = Year?.GetHashCode() ?? "Year".GetHashCode();
        var schoolInt = School?.GetHashCode() ?? "School".GetHashCode();
        var code = "SingleCourseJson".GetHashCode();
        return hashWithoutLastUpdate ^ hashCode ^ basePathInt ^ yearInt ^ schoolInt ^ code;
    }

    public int Compare(SingleCourseJson singleCourseJson)
    {
        if (Year != singleCourseJson.Year)
            return (Year ?? -1) < (singleCourseJson.Year ?? -1) ? -1 : 1;

        if (School != singleCourseJson.School) return School < singleCourseJson.School ? -1 : 1;

        if (BasePath != singleCourseJson.BasePath)
            return string.Compare(BasePath ?? "", singleCourseJson.BasePath ?? "", StringComparison.InvariantCulture);

        if (Link != singleCourseJson.Link)
            return string.Compare(Link ?? "", singleCourseJson.Link ?? "", StringComparison.InvariantCulture);

        if (Location != singleCourseJson.Location)
            return string.Compare(Location ?? "", singleCourseJson.Location ?? "", StringComparison.InvariantCulture);

        if (Name != singleCourseJson.Name)
            return string.Compare(Name ?? "", singleCourseJson.Name ?? "", StringComparison.InvariantCulture);

        return 0;
    }

    public bool Is(CourseTable courseTable)
    {
        return Name == courseTable.Title;
    }
}