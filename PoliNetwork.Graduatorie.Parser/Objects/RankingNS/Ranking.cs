#region

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
    public required List<CourseTable> ByCourse;
    public required MeritTable ByMerit;
    public required string Extra;
    public required DateTime LastUpdate;
    public required RankingOrder RankingOrder;
    public required RankingSummary RankingSummary;
    public required SchoolEnum School;
    public required RankingUrl Url;
    public required int Year;

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

    public RankingSummaryStudent GetRankingSummaryStudent()
    {
        return new RankingSummaryStudent(RankingOrder.Phase, School, Year, Url);
    }

    public static Ranking? FromJson(string fullPath)
    {
        // if (!File.Exists(fullPath)) return null;
        //
        // var str = File.ReadAllText(fullPath);
        // var ranking = JsonConvert.DeserializeObject<Ranking>(str, Culture.JsonSerializerSettings);
        // return ranking;

        // consider merging the two functions at some point
        return Utils.Transformer.ParserNS.Parser.ParseJsonRanking(fullPath);
    }


    public bool IsSimilarTo(Ranking ranking)
    {
        return Year == ranking.Year &&
               School == ranking.School &&
               RankingOrder.Phase == ranking.RankingOrder.Phase &&
               Extra == ranking.Extra &&
               Url.Url == ranking.Url.Url;
    }


    public string GetFilename()
    {
        var id = GetId();
        return $"{id}.json";
    }

    public string GetId()
    {
        var idList = new List<string>();

        var schoolShort = School.ToShortName();
        idList.Add(schoolShort);

        var yearStr = Year.ToString();
        idList.Add(yearStr);

        var orderId = RankingOrder.GetId();
        idList.Add(orderId);

        return string.Join("_", idList);
    }

    public List<SingleCourseJson> ToSingleCourseJson()
    {
        var result = new List<SingleCourseJson>();
        var schoolString = Enum.GetName(typeof(SchoolEnum), School);
        var courseTables = ByCourse;
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

        if (!forceReparse && equalsSaved) return;

        var rankingJsonString = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        File.WriteAllText(fullPath, rankingJsonString);
    }

    /***
     * Ottieni l'hash senza considerare il valore di LastUpdate
     */
    public int GetHashWithoutLastUpdate()
    {
        var i = "Ranking".GetHashCode();
        i ^= Extra.GetHashCode();
        i ^= RankingOrder.GetHashWithoutLastUpdate();
        i ^= RankingSummary.GetHashWithoutLastUpdate();
        i ^= School.GetHashCode();
        i ^= Url.GetHashWithoutLastUpdate();
        i ^= Year.GetHashCode();
        i ^= ByMerit.GetHashWithoutLastUpdate();
        i = ByCourse.Aggregate(i, (current, variable) =>
        {
            var hashWithoutLastUpdate = variable.GetHashWithoutLastUpdate();
            return current ^ hashWithoutLastUpdate;
        });

        return i;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Ranking);
    }

    public override int GetHashCode()
    {
        return GetHashWithoutLastUpdate();
    }
}