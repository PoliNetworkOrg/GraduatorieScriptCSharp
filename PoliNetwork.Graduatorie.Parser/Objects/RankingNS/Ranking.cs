using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;
using PoliNetwork.Graduatorie.Parser.Utils.Output;

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Ranking
{
    public List<CourseTable>? ByCourse;
    public MeritTable? ByMerit;
    public string? Extra;
    public DateTime LastUpdate;
    public string? Phase;
    public RankingSummary? RankingSummary;
    public SchoolEnum? School;
    public RankingUrl? Url;
    public int? Year;

    public RankingSummaryStudent GetRankingSummaryStudent()
    {
        return new RankingSummaryStudent(Phase, School, Year, Url);
    }


    /***
     * Ottieni l'hash senza considerare il valore di LastUpdate
     */
    public int GetHashWithoutLastUpdate()
    {
        var i = "Ranking".GetHashCode();
        i ^= Extra?.GetHashCode() ?? "Extra".GetHashCode();
        i ^= Phase?.GetHashCode() ?? "Phase".GetHashCode();
        i ^= RankingSummary?.GetHashWithoutLastUpdate() ?? "RankingSummary".GetHashCode();
        i ^= School?.GetHashCode() ?? "School".GetHashCode();
        i ^= Url?.GetHashWithoutLastUpdate() ?? "Url".GetHashCode();
        i ^= Year?.GetHashCode() ?? "Year".GetHashCode();
        i ^= ByMerit?.GetHashWithoutLastUpdate() ?? "ByMerit".GetHashCode();

        if (ByCourse == null)
            i ^= "ByCourse".GetHashCode();
        else
            i = ByCourse.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());

        return i;
    }


    public bool IsSimilarTo(Ranking ranking)
    {
        return Year == ranking.Year &&
               School == ranking.School &&
               Phase == ranking.Phase &&
               Extra == ranking.Extra &&
               Url?.Url == ranking.Url?.Url;
    }


    public void Merge(Ranking ranking)
    {
        LastUpdate = LastUpdate > ranking.LastUpdate ? LastUpdate : ranking.LastUpdate;
        Year ??= ranking.Year;
        Extra ??= ranking.Extra;
        School ??= ranking.School;
        Phase ??= ranking.Phase;
        ByCourse ??= ranking.ByCourse;
        ByMerit ??= ranking.ByMerit;
        Url ??= ranking.Url;
    }

    public string ConvertPhaseToFilename()
    {
        var s = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + "Z";
        var phase1 = Phase ?? s;
        return $"{phase1}.json".Replace(" ", "_");
    }

    public List<SingleCourseJson> ToSingleCourseJson()
    {
        var result = new List<SingleCourseJson>();
        var schoolString = School == null ? null : Enum.GetName(typeof(SchoolEnum), School);
        var courseTables = ByCourse;
        if (courseTables == null) return result;
        result.AddRange(courseTables.Select(variable => new SingleCourseJson
        {
            Link = ConvertPhaseToFilename(),
            Name = Phase,
            BasePath = schoolString + "/" + Year + "/",
            Year = Year,
            School = School,
            Location = variable.Location
        }));

        return result;
    }

    public List<StatsSingleCourseJson> ToStats()
    {
        return StatsSingleCourseJson.From(this);
    }

    public RankingSummary CreateSummary()
    {
        return RankingSummary.From(this);
    }

    public string GetPath()
    {
        return this.School.ToString() + "/" + this.Year.ToString() + "/" + this.Phase?.ToString();
    }
}