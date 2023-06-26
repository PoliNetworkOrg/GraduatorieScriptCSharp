using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class SingleCourseJson
{
    public string? Link;
    public string? Name;
}