using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Ranking
{
    private string? year;
    private string? school;
    private string? phase;
    private string? extra;
    private List<StudentResult>? byMerit;
    private List<StudentResult>? byId;
    private Dictionary<string, List<StudentResult>>? byDegree;
    private string? url;
    private DateTime lastUpdate;

    public bool IsSimilarTo(Ranking ranking)
    {
        return this.year == ranking.year && 
               this.school == ranking.school && 
               this.phase == ranking.phase &&
               this.extra == ranking.extra &&
               this.url == ranking.url;
    }
}