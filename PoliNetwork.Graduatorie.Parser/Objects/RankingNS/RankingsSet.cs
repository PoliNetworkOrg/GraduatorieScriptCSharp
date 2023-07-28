using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingsSet
{
    public DateTime? LastUpdate;
    public List<Ranking> Rankings;

    public RankingsSet()
    {
        Rankings = new List<Ranking>();
        LastUpdate = DateTime.UtcNow;
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

    public void Merge(RankingsSet set)
    {
        foreach (var ranking in set.Rankings) AddRanking(ranking);
    }
}