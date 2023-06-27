using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Transformer;

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