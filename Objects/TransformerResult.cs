using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class TransformerResult
{
    public HashSet<string>? PathFound;
    public RankingsSet? RankingsSet;

    public void AddFileRead(string fileContent, string filePath)
    {
        PathFound ??= new HashSet<string>();
        PathFound.Add(filePath);

        RankingsSet ??= new RankingsSet { LastUpdate = DateTime.Now };
        RankingsSet.ParseHtml(fileContent, RankingUrl.From(filePath));
    }
}
