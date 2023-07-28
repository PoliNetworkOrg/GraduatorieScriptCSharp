#region

using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Stats;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

public class OutputWriteUtil
{
    private readonly ArgsConfig _config;

    public OutputWriteUtil(ArgsConfig argsConfig)
    {
        _config = argsConfig;
    }

    public void SaveOutputs(RankingsSet? rankingsSet, DateFound dateFound)
    {
        var outFolder = Path.Join(_config.DataFolder, Constants.OutputFolder);
        IndexJsonBase.IndexesWrite(rankingsSet, outFolder, _config);
        StatsJson.Write(outFolder, rankingsSet, _config);
        HashMatricoleWrite.Write(rankingsSet, outFolder);

        dateFound.WriteToFile(_config.DataFolder);
    }
}