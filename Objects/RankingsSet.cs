using Newtonsoft.Json;

namespace GraduatorieScript.Objects;
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingsSet
{
    public List<Ranking> Rankings;
    public DateTime? LastUpdate;

    public RankingsSet() {
      Rankings = new List<Ranking>();
      LastUpdate = DateTime.Now;
    }

    public static RankingsSet Merge(List<RankingsSet?> list)
    {
        var rankingsSet = new RankingsSet
        {
            LastUpdate = list.Max(x => x?.LastUpdate ?? DateTime.Now),
            Rankings = new List<Ranking>()
        };

        foreach (var set in list)
        {
          if(set != null) rankingsSet.MergeSet(set);
        }

        return rankingsSet;
    }

    private void MergeSet(RankingsSet rankingsSet)
    {
        foreach (var ranking in rankingsSet.Rankings)
        {
            this.AddRanking(ranking);
        }
    }

    private void AddRanking(Ranking ranking)
    {
        var alreadyPresent = this.Contains(ranking);
        if (!alreadyPresent)
        {
            this.Rankings.Add(ranking);
        }
    }

    private bool Contains(Ranking ranking)
    {
        return this.Rankings.Any(v => v.IsSimilarTo(ranking));
    }

    public void AddFileRead(string fileContent)
    {
        //todo: da un testo formattato in html, ottenere la graduatoria o ogni altra informazione 
        //e aggiungerla alla classe attuale, evitando ripetizioni

        // check if exists page
        throw new NotImplementedException();
    }
}
