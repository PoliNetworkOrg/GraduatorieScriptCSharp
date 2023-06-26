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
        var byMeritRows = ranking.ByMerit?.Rows;
        var results = CalculateResultsScores(byMeritRows);

        var rankingSummary = new RankingSummary
        {
            HowManyCanEnroll = byMeritRows?.Count(x => x.canEnroll),
            HowManyStudents = byMeritRows?.Count,
            ResultsSummarized = results
        };

        return rankingSummary;
    }

    private static Dictionary<int, int>? CalculateResultsScores(IReadOnlyCollection<StudentResult>? byMeritRows)
    {
        if (byMeritRows == null) return null;

        var results = new Dictionary<int, int>();
        var enumerable = byMeritRows.Select(variable => (int)Math.Round(variable.result));
        foreach (var score in enumerable)
        {
            results.TryAdd(score, 0);
            results[score] += 1;
        }

        return results;
    }
}