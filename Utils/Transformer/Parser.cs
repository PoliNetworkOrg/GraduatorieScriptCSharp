using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Transformer;

public static class Parser
{
    public static TransformerResult ParseHtmlFiles(string? baseFolder)
    {
        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        throw new NotImplementedException();
    }

    public static RankingsSet ParseWeb(IEnumerable<string> rankingsLinks)
    {
        //download delle graduatorie, ricorsivamente, e inserimento nel rankingsSet
        var rankingsSet = new RankingsSet
        {
            lastUpdate = DateTime.Now,
            Rankings = new List<Ranking>()
        };
        foreach (var link in rankingsLinks.Where(link => !string.IsNullOrEmpty(link)))
        {
     
            rankingsSet.Rankings.Add(Web.Scraper.Download(link));
        }
        return rankingsSet;
    }

    public static RankingsSet? ParseLocalJson(string jJsonPath)
    {
        if (string.IsNullOrEmpty(jJsonPath))
            return null;

        if (File.Exists(jJsonPath) == false)
            return null;

        var fileContent = File.ReadAllText(jJsonPath);
        if (string.IsNullOrEmpty(fileContent))
            return null;

        var rankingsSet = Newtonsoft.Json.JsonConvert.DeserializeObject<RankingsSet>(fileContent);
        return rankingsSet;
    }
}