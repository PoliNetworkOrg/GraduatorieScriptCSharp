using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class TransformerResult
{
    public HashSet<string?>? pathFound;
    public RankingsSet? RankingsSet;

    public void AddFileRead(string fileContent, string filePath)
    {
        this.pathFound ??= new HashSet<string?>();
        this.pathFound.Add(filePath);

        this.RankingsSet ??= new RankingsSet() { LastUpdate = DateTime.Now };
        this.RankingsSet.AddFileRead(fileContent);
    }
}