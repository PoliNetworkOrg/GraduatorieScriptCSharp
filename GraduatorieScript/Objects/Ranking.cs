using System.Globalization;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.Json;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
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
        //todo: unire i campi correnti con quello ricevuto


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

    public SingleCourseJson ConvertToSingleSource() =>
        new()
        {
            Link = ConvertPhaseToFilename(),
            Name = Phase
        };

    public StatsSingleJson ToStats()
    {
        return StatsSingleJson.From(this);
    }
}