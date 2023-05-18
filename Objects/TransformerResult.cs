using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class TransformerResult
{
    public List<string> pathFound;
    public RankingsSet RankingsSet;
}