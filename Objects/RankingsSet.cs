using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
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
        var rankingsSet = new RankingsSet
        {
            LastUpdate = sets.Max(x => x?.LastUpdate ?? DateTime.Now),
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
        if (!alreadyPresent) Rankings.Add(ranking);
    }

    public bool Contains(Ranking ranking)
    {
        return Rankings.Any(v => v.IsSimilarTo(ranking));
    }

}
