using GraduatorieScript.Enums;
using HtmlAgilityPack;
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

    public HtmlDocument GetHtml()
    {
        throw new NotImplementedException();
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
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MeritTable
{
    public List<string>? Headers;
    public List<StudentResult>? Rows;
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTable : MeritTable
{
    public string? Location;
    public List<string>? Sections;
    public string? Title;
}