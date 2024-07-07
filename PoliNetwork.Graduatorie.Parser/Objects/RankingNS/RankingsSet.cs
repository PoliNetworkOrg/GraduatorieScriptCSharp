#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingsSet
{
    public DateTime? LastUpdate = DateTime.UtcNow;
    public List<Ranking> Rankings = new();

    public void Merge(RankingsSet newSet)
    {
        foreach (var ranking in newSet.Rankings)
        {
            var alreadyPresent = Rankings.Any(v => v.IsSimilarTo(ranking));
            if (alreadyPresent) continue;
            Rankings.Add(ranking);
                    
            if (LastUpdate == null || ranking.LastUpdate.Date > LastUpdate?.Date) 
                LastUpdate = ranking.LastUpdate;
        }
    }

    public void WriteAllRankings(string outFolder, bool forceReparse = false)
    {
        foreach (var ranking in Rankings) ranking.WriteAsJson(outFolder, forceReparse);
    }
}