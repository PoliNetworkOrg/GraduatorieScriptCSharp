#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class DateFound
{
    public const string PathFileName = "dateFound.json";
    public SortedDictionary<string, DateTime?>? FirstDate;

    public void WriteToFile(string dataFolder)
    {
        var s = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        var path = Path.Join(dataFolder, PathFileName);
        File.WriteAllText(path, s);
    }

    public static DateTime? MinDateTime(DateTime? date1, DateTime? date2)
    {
        if (date1 == null && date2 == null)
            return null;
        if (date1 == null)
            return date2;
        if (date2 == null)
            return date1;

        return date1 < date2 ? date1 : date2;
    }

    public void UpdateDateFound(Ranking variable)
    {
        var path = variable.GetPath().Trim();
        var minDateTime = GetMinTime(variable, path);
        SetDate(path, minDateTime);
    }

    private void SetDate(string path, DateTime? minDateTime)
    {
        FirstDate ??= new SortedDictionary<string, DateTime?>();
        FirstDate[path] = minDateTime;
    }

    private DateTime? GetMinTime(Ranking variable, string path)
    {
        FirstDate ??= new SortedDictionary<string, DateTime?>();
        var dateTime = new DateTime(variable.Year ?? DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
        var tryGetValue = TryGetValue(FirstDate, path);
        var minDateTime = tryGetValue.Item2 switch
        {
            true => MinDateTime(dateTime, tryGetValue.Item1),
            _ => dateTime
        };
        return minDateTime;
    }

    public static Tuple<T?, bool> TryGetValue<T>(SortedDictionary<string, T> d, string path)
    {
        var tryGetValue = d.TryGetValue(path, out var oldValue);
        return new Tuple<T?, bool>(oldValue, tryGetValue);
    }
}