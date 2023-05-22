using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;

namespace GraduatorieScript.Utils.Transformer;

public static class Parser
{
    public static TransformerResult? ParseHtmlFiles(string? baseFolder)
    {
        if (string.IsNullOrEmpty(baseFolder))
            return null;

        var transformerResult = new TransformerResult();

        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        var files = Directory.GetFiles(baseFolder, "*.html", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var fileRelativePath = file.Split(baseFolder)[1];

            // ignore because this is the file built 
            // by previous script which is useless for this one
            // (and it also breaks our logic)
            if(fileRelativePath == "index.html") continue; 

            var html = File.ReadAllText(file);
            var url = $"http://{Constants.RisultatiAmmissionePolimiIt}{fileRelativePath}";
            // no need to check if url is online
            // because the html is already stored

            transformerResult.AddFileRead(html, url);
        }

        return transformerResult;
    }

    public static RankingsSet ParseWeb(IEnumerable<RankingUrl> rankingsUrls)
    {
        //download delle graduatorie, ricorsivamente, e inserimento nel rankingsSet
        var rankingsSet = new RankingsSet
        {
            LastUpdate = DateTime.Now,
            Rankings = new List<Ranking>()
        };

        foreach (var r in rankingsUrls)
        {
            var download = Scraper.Download(r.url);
            if (download != null) rankingsSet.Rankings.Add(download);
        }

        return rankingsSet;
    }

    public static RankingsSet? ParseLocalJson(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            return null;

        var fileContent = File.ReadAllText(jsonPath);
        if (string.IsNullOrEmpty(fileContent))
            return null;

        var rankingsSet = JsonConvert.DeserializeObject<RankingsSet>(fileContent);
        return rankingsSet;
    }
}
