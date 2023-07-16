using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

public static class OutputWriteUtil
{
    public static void SaveOutputs(string? dataFolder, RankingsSet? rankingsSet, DateFound? dateFound)
    {
        if (string.IsNullOrEmpty(dataFolder))
            return;

        var outFolder = Path.Join(dataFolder, Constants.OutputFolder);
        IndexJsonBase.IndexesWrite(rankingsSet, outFolder);
        StatsJson.Write(outFolder, rankingsSet);
        HashMatricoleWrite.Write(rankingsSet, outFolder);
        dateFound?.WriteToFile(dataFolder);
    }
}