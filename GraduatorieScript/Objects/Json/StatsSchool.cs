using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int? NumStudents;
}