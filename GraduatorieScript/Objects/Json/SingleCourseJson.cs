using Newtonsoft.Json;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class SingleCourseJson
{
    public string? Name;
    public string? Link;
}