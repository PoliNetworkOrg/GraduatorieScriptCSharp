using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MeritTable
{
    public List<string>? Headers;
    public List<StudentResult>? Rows;
}