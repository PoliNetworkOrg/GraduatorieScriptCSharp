using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StatsSchool
{
    public List<StatsSingleJson>? List;
    public int? NumStudents;

    public StatsSchool()
    {
        List = new List<StatsSingleJson>();
    }
}