using GraduatorieScript.Enums;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Ranking
{
    public List<CourseTable>? byCourse;
    public MeritTable? byMerit;
    public string? extra;
    public DateTime LastUpdate;
    public string? phase;
    public RankingSummary? RankingSummary;
    public SchoolEnum? school;
    public RankingUrl? Url;
    public int? year;

    public bool IsSimilarTo(Ranking ranking)
    {
        return year == ranking.year &&
               school == ranking.school &&
               phase == ranking.phase &&
               extra == ranking.extra &&
               Url?.Url == ranking.Url?.Url;
    }


    public void Merge(Ranking ranking)
    {
        //todo: unire i campi correnti con quello ricevuto


        LastUpdate = LastUpdate > ranking.LastUpdate ? LastUpdate : ranking.LastUpdate;
        year ??= ranking.year;
        extra ??= ranking.extra;
        school ??= ranking.school;
        phase ??= ranking.phase;
        byCourse ??= ranking.byCourse;
        byMerit ??= ranking.byMerit;
        Url ??= ranking.Url;
    }

    public string ConvertPhaseToFilename()
    {
        return $"{phase ?? DateTime.Now.ToString()}.json".Replace(" ", "_");
    }
}
