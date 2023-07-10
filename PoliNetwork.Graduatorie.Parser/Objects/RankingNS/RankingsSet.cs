using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

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
        LastUpdate = DateTime.Now;
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