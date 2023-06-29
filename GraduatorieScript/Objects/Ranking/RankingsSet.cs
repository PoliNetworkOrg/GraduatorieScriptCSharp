using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingsSet
{
    public DateTime? LastUpdate;
    public List<Ranking> Rankings;

    public RankingsSet()
    {
        Rankings = new List<Ranking>();
        LastUpdate = DateTime.Now;
    }

    public static RankingsSet Merge(params RankingsSet?[] sets)
    {
        var fixedSets = sets.Where(set => set is not null);
        var rankingsSet = new RankingsSet
        {
            LastUpdate = fixedSets.Max(x => x!.LastUpdate ?? DateTime.Now),
            Rankings = new List<Ranking>()
        };

        foreach (var set in sets)
            if (set != null)
                rankingsSet.MergeSet(set);

        return rankingsSet;
    }

    public void MergeSet(RankingsSet rankingsSet)
    {
        foreach (var ranking in rankingsSet.Rankings) AddRanking(ranking);
    }

    public void AddRanking(Ranking ranking)
    {
        var alreadyPresent = Contains(ranking);
        if (!alreadyPresent)
            Rankings.Add(ranking);

        if (LastUpdate == null || ranking.LastUpdate.Date > LastUpdate?.Date) LastUpdate = ranking.LastUpdate;
    }

    public bool Contains(Ranking ranking)
    {
        return Rankings.Any(v => v.IsSimilarTo(ranking));
    }
}
