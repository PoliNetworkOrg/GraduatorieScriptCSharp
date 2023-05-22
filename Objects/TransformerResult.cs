using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class TransformerResult
{
    public HashSet<string>? pathFound;
    public RankingsSet? RankingsSet;

    public void AddFileRead(string fileContent, string filePath)
    {
        pathFound ??= new HashSet<string>();
        pathFound.Add(filePath);

        RankingsSet ??= new RankingsSet { LastUpdate = DateTime.Now };
        RankingsSet.ParseHtml(fileContent, RankingUrl.From(filePath));
    }
}
