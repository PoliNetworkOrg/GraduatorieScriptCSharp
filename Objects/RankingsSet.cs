using GraduatorieScript.Enums;
using Newtonsoft.Json;
using HtmlAgilityPack;
using GraduatorieScript.Extensions;

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

    public static RankingsSet Merge(IEnumerable<RankingsSet?> list)
    {
        var rankingsSets = list.ToList();
        var rankingsSet = new RankingsSet
        {
            LastUpdate = rankingsSets.Max(x => x?.LastUpdate ?? DateTime.Now),
            Rankings = new List<Ranking>()
        };

        foreach (var set in rankingsSets)
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

    public static void ParseHtml(string html, RankingUrl url)
    {
        if (string.IsNullOrEmpty(html) || url.page == Page.Unknown)
            return;

        //todo: da un testo formattato in html, ottenere la graduatoria o ogni altra informazione 
        //e aggiungerla alla classe attuale, evitando ripetizioni

        var page = new HtmlDocument();
        page.LoadHtml(html);
        var doc = page.DocumentNode;

        var intestazione = doc
            .GetElementsByClassName("intestazione")
            .Select(el => el.InnerText)
            .First(text => text.Contains("Politecnico"));

        if(string.IsNullOrEmpty(intestazione)) return;
        
        Console.WriteLine($"{url.url} {url.page} valid");

        throw new NotImplementedException(); // just as a reminder
    }
}
