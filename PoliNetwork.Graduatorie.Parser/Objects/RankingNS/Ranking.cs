#region

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

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Ranking
{
    public List<CourseTable>? ByCourse;
    public MeritTable? ByMerit;
    public string? Extra;
    public DateTime LastUpdate;
    public RankingOrder? RankingOrder;
    public RankingSummary? RankingSummary;
    public SchoolEnum? School;
    public RankingUrl? Url;
    public int? Year;

    public RankingSummaryStudent GetRankingSummaryStudent()
    {
        return new RankingSummaryStudent(RankingOrder?.Phase, School, Year, Url);
    }


    /***
     * Ottieni l'hash senza considerare il valore di LastUpdate
     */
    public int GetHashWithoutLastUpdate()
    {
        var i = "Ranking".GetHashCode();
        i ^= Extra?.GetHashCode() ?? "Extra".GetHashCode();
        i ^= RankingOrder?.GetHashWithoutLastUpdate() ?? "RankingOrder".GetHashCode();
        i ^= RankingSummary?.GetHashWithoutLastUpdate() ?? "RankingSummary".GetHashCode();
        i ^= School?.GetHashCode() ?? "School".GetHashCode();
        i ^= Url?.GetHashWithoutLastUpdate() ?? "Url".GetHashCode();
        i ^= Year?.GetHashCode() ?? "Year".GetHashCode();
        var iMerit = ByMerit?.GetHashWithoutLastUpdate();
        i ^= GetHashFromListHash(iMerit) ?? "ByMerit".GetHashCode();
        

        if (ByCourse == null)
            i ^= "ByCourse".GetHashCode();
        else
            i = ByCourse.Aggregate(i, (current, variable) =>
            {
                var hashWithoutLastUpdate = variable.GetHashWithoutLastUpdate();
                var iList = GetHashFromListHash(hashWithoutLastUpdate) ?? "empty".GetHashCode();
                return current ^ iList;
            });

        return i;
    }

    public static int? GetHashFromListHash(IReadOnlyCollection<int>? iMerit)
    {
        if (iMerit == null)
            return null;
        if (iMerit.Count == 0)
            return null;

        return iMerit.Aggregate(0, (current, variable) => current ^ variable);
    }

    public int GetHashWithoutLastUpdateTableMerit()
    {
        var i = "RankingTableMerit".GetHashCode();

        var hashWithoutLastUpdate = GetHashFromListHash(ByMerit?.GetHashWithoutLastUpdate());
        i ^= hashWithoutLastUpdate ?? "ByMerit".GetHashCode();

        return i;
    }


    public int GetHashWithoutLastUpdateTableCourse()
    {
        var i = "RankingTableCourse".GetHashCode();

        if (ByCourse == null)
            i ^= "ByCourse".GetHashCode();
        else
            i = ByCourse.Aggregate(i, (current, variable) =>
            {
                var hashWithoutLastUpdate = variable.GetHashWithoutLastUpdate();
                var hashFromListHash = GetHashFromListHash(hashWithoutLastUpdate) ?? "empty2".GetHashCode();
                return current ^ hashFromListHash;
            });

        return i;
    }

    public int GetHashWithoutLastUpdateInfo()
    {
        var i = "RankingInfo".GetHashCode();
        i ^= Extra?.GetHashCode() ?? "Extra".GetHashCode();
        i ^= RankingOrder?.GetHashWithoutLastUpdate() ?? "RankingOrder".GetHashCode();
        i ^= RankingSummary?.GetHashWithoutLastUpdate() ?? "RankingSummary".GetHashCode();
        i ^= School?.GetHashCode() ?? "School".GetHashCode();
        i ^= Url?.GetHashWithoutLastUpdate() ?? "Url".GetHashCode();
        i ^= Year?.GetHashCode() ?? "Year".GetHashCode();
        return i;
    }


    public bool IsSimilarTo(Ranking ranking)
    {
        return Year == ranking.Year &&
               School == ranking.School &&
               RankingOrder?.Phase == ranking.RankingOrder?.Phase &&
               Extra == ranking.Extra &&
               Url?.Url == ranking.Url?.Url;
    }


    public void Merge(Ranking ranking)
    {
        LastUpdate = LastUpdate > ranking.LastUpdate ? LastUpdate : ranking.LastUpdate;
        Year ??= ranking.Year;
        Extra ??= ranking.Extra;
        School ??= ranking.School;
        MergeRankingOrder(ranking);
        ByCourse ??= ranking.ByCourse;
        ByMerit ??= ranking.ByMerit;
        Url ??= ranking.Url;
    }

    private void MergeRankingOrder(Ranking ranking)
    {
        if (RankingOrder == null)
            RankingOrder = ranking.RankingOrder;
        else
            RankingOrder.Merge(ranking.RankingOrder);
    }

    public string ConvertPhaseToFilename()
    {
        var s = DateTime.UtcNow.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + "Z";
        var phase1 = RankingOrder?.Phase ?? s;
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
            Name = RankingOrder?.Phase,
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
        return School + "/" + Year + "/" + RankingOrder?.Phase;
    }

    public MeritTable? GetMerit()
    {
        return this.ByMerit;
    }

    public List<CourseTable>? GetTableCourse()
    {
        return this.ByCourse;
    }
}