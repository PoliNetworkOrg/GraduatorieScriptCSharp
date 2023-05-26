using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;

namespace GraduatorieScript.Main;

public static class Program
{
// See https://aka.ms/new-console-template for more information

    public static void Main()
    {
        var mt = new Metrics();

        var baseFolder = PathUtils.FindDocsFolder();
        Console.WriteLine($"[INFO] baseFolder: {baseFolder}");

//find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll);

//print links found
        foreach (var r in rankingsUrls) Console.WriteLine($"[DEBUG] valid url found: {r.url}");

// todo: handle when baseFolder is null 
        var outputJsonPath = Path.Join(baseFolder, Constants.OutputJsonFilename);

//nella cartella trovata, leggere e analizzare gli eventuali file .html
        var rankingsSetFromHtmls = Parser.FindParseHtmls(baseFolder);

//estraiamo i risultati dal web
        var rankingsSetFromWeb = Parser.ParseWeb(rankingsUrls);

//estraiamo i risultati da un eventuale json locale
        var rankingsSetFromLocalJson = Parser.ParseLocalJson(outputJsonPath);

//uniamo i dataset (quello dall'html, quello dal json locale, quello dal web)
        var fullRankingsSet = RankingsSet.Merge(rankingsSetFromHtmls, rankingsSetFromWeb, rankingsSetFromLocalJson);

//ottenere un json 
        var stringJson = JsonConvert.SerializeObject(fullRankingsSet);

//scriviamolo su disco
        File.WriteAllText(outputJsonPath, stringJson);

//eliminare i suddetti file html
/* if (transformerResult?.pathFound != null) */
/*     FileUtils.TryBulkDelete(transformerResult.pathFound); */
// ^^ this must be wrong
    }
}