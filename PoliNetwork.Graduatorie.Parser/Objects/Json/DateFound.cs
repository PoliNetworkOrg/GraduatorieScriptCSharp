using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DateFound
{
    public const string PathFileName = "dateFound.json";
    public Dictionary<string, DateTime?>? FirstDate;

    public void WriteToFile(string dataFolder)
    {
        var s = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        var path = Path.Join(dataFolder, PathFileName);
        File.WriteAllText(path, s);
    }


    public static DateTime MinDateTime(DateTime date1, DateTime date2)
    {
        return date1 < date2 ? date1 : date2;
    }

    public void UpdateDateFound(Ranking variable)
    {
        var path = variable.GetPath().Trim();

        FirstDate ??= new Dictionary<string, DateTime?>();

        var dateTime = new DateTime(variable.Year ?? DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        if (FirstDate.TryGetValue(path, out var oldValue))
        {
            if (oldValue == null)
                FirstDate[path] = dateTime;
            else
                FirstDate[path] = MinDateTime(dateTime, oldValue.Value);
        }
        else
        {
            FirstDate[path] = dateTime;
        }
    }
}