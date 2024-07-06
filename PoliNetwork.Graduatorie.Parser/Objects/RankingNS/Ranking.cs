#region

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
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
public class Ranking : IComparable<Ranking>, IEquatable<Ranking>
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


    public int CompareTo(Ranking? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;

        return string.Compare(GetId(), other.GetId(), StringComparison.Ordinal);
    }


    public bool Equals(Ranking? other)
    {
        if (other == null) return false;
        return GetHashWithoutLastUpdate() == other.GetHashWithoutLastUpdate();
    }


    public static Ranking? FromJson(string fullPath)
    {
        if (!File.Exists(fullPath)) return null;

        var str = File.ReadAllText(fullPath);
        var ranking = JsonConvert.DeserializeObject<Ranking>(str, Culture.JsonSerializerSettings);
        return ranking;
    }

    public RankingSummaryStudent GetRankingSummaryStudent()
    {
        return new RankingSummaryStudent(RankingOrder?.Phase, School, Year, Url);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Ranking);
    }

    public override int GetHashCode()
    {
        return GetHashWithoutLastUpdate();
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

    public string GetFilename()
    {
        var id = GetId();
        return $"{id}.json";
    }

    public string GetId()
    {
        var idList = new List<string>();

        var schoolShort = School?.ToShortName();
        if (schoolShort != null) idList.Add(schoolShort);

        var yearStr = Year.ToString();
        if (yearStr != null) idList.Add(yearStr);

        var orderId = RankingOrder?.GetId();
        if (orderId != null) idList.Add(orderId);

        var fallback = DateTime.UtcNow.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + "Z";
        if (idList.Count == 0) idList.Add(fallback);

        return string.Join("_", idList);
    }

    public List<SingleCourseJson> ToSingleCourseJson()
    {
        var result = new List<SingleCourseJson>();
        var schoolString = School == null ? null : Enum.GetName(typeof(SchoolEnum), School);
        var courseTables = ByCourse;
        if (courseTables == null) return result;
        result.AddRange(courseTables.Select(variable => new SingleCourseJson
        {
            Link = GetFilename(),
            Id = GetId(),
            BasePath = schoolString + "/" + Year + "/",
            Year = Year,
            School = School,
            Location = variable.Location,
            RankingOrder = RankingOrder
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

    public string GetBasePath(string outFolder = "")
    {
        return Path.Join(outFolder, $"{School}/{Year}/");
    }

    public string GetFullPath(string outFolder = "")
    {
        return Path.Join(GetBasePath(outFolder), GetFilename());
    }

    public void WriteAsJson(string outFolder, bool forceReparse = false)
    {
        var folderPath = GetBasePath(outFolder);
        Directory.CreateDirectory(folderPath);

        var fullPath = GetFullPath(outFolder);

        var savedRanking = FromJson(fullPath);
        var equalsSaved = savedRanking != null && Equals(savedRanking);

        if (forceReparse || equalsSaved || savedRanking == null)
        {
            var rankingJsonString = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
            File.WriteAllText(fullPath, rankingJsonString);
        }
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
        i ^= iMerit ?? "ByMerit".GetHashCode();


        if (ByCourse == null)
            i ^= "ByCourse".GetHashCode();
        else
            i = ByCourse.Aggregate(i, (current, variable) =>
            {
                var hashWithoutLastUpdate = variable.GetHashWithoutLastUpdate();
                return current ^ hashWithoutLastUpdate;
            });

        return i;
    }
}