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

        
        this.LastUpdate = this.LastUpdate > ranking.LastUpdate ? this.LastUpdate : ranking.LastUpdate;
        this.year ??= ranking.year;
        this.extra ??= ranking.extra;
        this.school ??= ranking.school;
        this.phase ??= ranking.phase;
        this.byCourse ??= ranking.byCourse;
        this.byMerit ??= ranking.byMerit;
        this.Url ??= ranking.Url;
    }
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MeritTable {
    public List<string>? Headers;
    public List<StudentResult>? Rows;
}

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTable : MeritTable{
    public string? Title;
    public string? Location;
    public List<string>? Sections;
}
