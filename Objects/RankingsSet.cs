using Newtonsoft.Json;

namespace GraduatorieScript.Objects;
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingsSet
{
    public List<Ranking>? Rankings;
    public DateTime? LastUpdate;

    public static RankingsSet Merge(List<RankingsSet?> list)
    {
        var rankingsResult = new List<Ranking>();

        foreach (var rankingsSet in list)
        {
            MergeSingleList(rankingsSet, rankingsResult);
        }
        
        var result = new RankingsSet
        {
            LastUpdate = list.Max(x => x?.LastUpdate ?? DateTime.Now),
            Rankings = rankingsResult
        };
        return result;
    }

    private static void MergeSingleList(RankingsSet? rankingsSet, List<Ranking> rankingsResult)
    {
        var rankingsSetRankings = rankingsSet?.Rankings;
        if (rankingsSetRankings == null) return;
        foreach (var ranking in rankingsSetRankings)
        {
            MergeSingleRanking(rankingsResult, ranking);
        }
    }

    private static void MergeSingleRanking(ICollection<Ranking> rankingsResult, Ranking ranking)
    {
        var alreadyPresent = CheckIfAlreadyPresent(rankingsResult, ranking);
        if (!alreadyPresent)
        {
            rankingsResult.Add(ranking);
        }
    }

    private static bool CheckIfAlreadyPresent(IEnumerable<Ranking> rankingsResult, Ranking ranking)
    {
        return rankingsResult.Any(v => v.IsSimilarTo(ranking));
    }
}