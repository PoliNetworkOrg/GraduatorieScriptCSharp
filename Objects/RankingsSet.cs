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

    public static RankingsSet Merge(List<RankingsSet?> list)
    {
        var rankingsSet = new RankingsSet
        {
            LastUpdate = list.Max(x => x?.LastUpdate ?? DateTime.Now),
            Rankings = new List<Ranking>()
        };

        foreach (var set in list)
            if (set != null)
                rankingsSet.MergeSet(set);

        return rankingsSet;
    }

    private void MergeSet(RankingsSet rankingsSet)
    {
        foreach (var ranking in rankingsSet.Rankings) AddRanking(ranking);
    }

    private void AddRanking(Ranking ranking)
    {
        var alreadyPresent = Contains(ranking);
        if (!alreadyPresent) Rankings.Add(ranking);
    }

    private bool Contains(Ranking ranking)
    {
        return Rankings.Any(v => v.IsSimilarTo(ranking));
    }

    public void AddFileRead(string fileContent)
    {
        if (string.IsNullOrEmpty(fileContent))
            return;

        //todo: da un testo formattato in html, ottenere la graduatoria o ogni altra informazione 
        //e aggiungerla alla classe attuale, evitando ripetizioni

        // check if exists page (controllare se il file html in ingresso abbia un senso o sia inutile)
        throw new NotImplementedException();
    }
}