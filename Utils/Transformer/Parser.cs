using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Transformer;

public static class Parser
{
    public static TransformerResult? ParseHtmlFiles(string? baseFolder)
    {
        if (string.IsNullOrEmpty(baseFolder))
            return null;

        var transformerResult = new TransformerResult();
        
        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        var files = Directory.GetFiles(baseFolder, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileContent = File.ReadAllText(file);
            transformerResult.AddFileRead(fileContent, file);
        }

        return transformerResult;
    }

    public static RankingsSet ParseWeb(IEnumerable<string?> rankingsLinks)
    {
        //download delle graduatorie, ricorsivamente, e inserimento nel rankingsSet
        var rankingsSet = new RankingsSet
        {
            LastUpdate = DateTime.Now,
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