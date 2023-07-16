using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Utils;

public static class DateFoundUtil
{
    public static DateFound? GetDateFound(ArgsConfig argsConfig, RankingsSet? rankingsSet)
    {
        var dateFound = GetDateFoundFromFile(argsConfig.DataFolder);
        dateFound = UpdateDateFound(rankingsSet, dateFound);
        return dateFound;
    }

    private static DateFound? UpdateDateFound(RankingsSet? rankingsSet, DateFound? dateFound)
    {
        dateFound ??= new DateFound();

        ;
        var rankingsSetRankings = rankingsSet?.Rankings;
        if (rankingsSetRankings == null) return dateFound;
        foreach (var variable in rankingsSetRankings) dateFound.UpdateDateFound(variable);

        return dateFound;
    }

    private static DateFound? GetDateFoundFromFile(string? argsConfigDataFolder)
    {
        if (string.IsNullOrEmpty(argsConfigDataFolder))
            return null;

        var path = Path.Join(argsConfigDataFolder, DateFound.PathFileName);
        if (!File.Exists(path))
            return null;

        var content = File.ReadAllText(path);
        try
        {
            return JsonConvert.DeserializeObject<DateFound>(content);
        }
        catch
        {
            return null;
        }
    }
}