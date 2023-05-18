using Newtonsoft.Json;

namespace GraduatorieScript.Objects;
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingsSet
{
    private List<Ranking> Rankings;
    private DateTime lastUpdate;

    public static RankingsSet Merge(List<RankingsSet?> list)
    {
        var rankingsResult = new List<Ranking>();

        foreach (var rankingsSet in list)
        {
            var rankingsSetRankings = rankingsSet?.Rankings;
            if (rankingsSetRankings == null) continue;
            foreach (var ranking in rankingsSetRankings)
            {
                var alreadyPresent = CheckIfAlreadyPresent(rankingsResult, ranking);
                if (!alreadyPresent)
                {
                    rankingsResult.Add(ranking);
                }
            }
        }
        
        RankingsSet result = new RankingsSet
        {
            lastUpdate = list.Max(x => x?.lastUpdate ?? DateTime.Now),
            Rankings = rankingsResult
        };
        return result;
    }

    private static bool CheckIfAlreadyPresent(List<Ranking> rankingsResult, Ranking ranking)
    {
        return rankingsResult.Any(v => v.IsSimilarTo(ranking));
    }
}