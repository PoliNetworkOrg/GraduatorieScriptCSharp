using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Utils;

public static class DateFoundUtil
{
    public static DateFound? GetDateFound(ArgsConfig argsConfig, IEnumerable<RankingUrl> rankingsSet)
    {
        DateFound? dateFound = GetDateFoundFromFile(argsConfig.DataFolder);
        dateFound = UpdateDateFound(rankingsSet, dateFound);
        return dateFound;
    }

    private static DateFound? UpdateDateFound(IEnumerable<RankingUrl> rankingsSet, DateFound? dateFound)
    {
        dateFound ??= new DateFound();

        ;
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