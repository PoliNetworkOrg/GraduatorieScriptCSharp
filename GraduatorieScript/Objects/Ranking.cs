using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Ranking
{
    private string? school;
    private string? year;
    private string? phase;
    private string? url;
    private Dictionary<string, List<StudentResult>>? byCourse;
    private List<StudentResult>? byMerit;
    private string? extra;
    public DateTime LastUpdate;

    public bool IsSimilarTo(Ranking ranking)
    {
        return year == ranking.year &&
               school == ranking.school &&
               phase == ranking.phase &&
               extra == ranking.extra &&
               url == ranking.url;
    }
}
