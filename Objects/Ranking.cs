using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Ranking
{
    private string year;
    private string school;
    private string phase;
    private string extra;
    private List<StudentResult> byMerit;
    private List<StudentResult> byId;
    private Dictionary<string, List<StudentResult>> byDegree;
    private string url;
    private DateTime lastUpdate;
}