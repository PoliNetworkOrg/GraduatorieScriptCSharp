using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;

namespace PoliNetwork.Graduatorie.Parser.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DateFound
{
    public Dictionary<string, DateTime?>? FirstDate;

    public const string PathFileName = "dateFound.json";

    public void WriteToFile(string dataFolder)
    {
        var s = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        var path = Path.Join(dataFolder, PathFileName);
        File.WriteAllText(path, s);
    }
}