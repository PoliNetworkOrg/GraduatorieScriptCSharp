using PoliNetwork.Graduatorie.Common.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

namespace PoliNetwork.Graduatorie.Parser.Utils.Transformer;

public static class StatsCalculate
{
    public static void CalculateStats(Ranking ranking)
    {
        ranking.RankingSummary = CalculateStatsSingle(ranking);
    }

    private static RankingSummary CalculateStatsSingle(Ranking ranking)
    {
        return ranking.CreateSummary();
    }
}