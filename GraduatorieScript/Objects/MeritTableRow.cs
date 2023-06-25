using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MeritTableRow
{
    public bool canEnroll;
    public string? canEnrollInto;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public int position;
    public decimal result;
}