using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;

namespace GraduatorieScript.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new SampleNuGet.Utils.Metrics();

        var baseFolder = args.Length > 0 && !string.IsNullOrEmpty(args[0]) ? args[0] : PathUtils.FindFolder(Constants.FolderToFind);
        Console.WriteLine($"[INFO] baseFolder [1]: {baseFolder}");

        if (string.IsNullOrEmpty(baseFolder))
            baseFolder = PathUtils.CreateAndReturnDocsFolder(Constants.FolderToFind);
        Console.WriteLine($"[INFO] baseFolder [2]: {baseFolder}");

        if (string.IsNullOrEmpty(baseFolder))
        {
            Console.WriteLine("[INFO] baseFolder is null. Abort.");
            return;
        }

        //find links from web
//        var rankingsUrls = mt.Execute(LinksFind.GetAll);
        RankingUrl[] rankingsUrls = { RankingUrl.From("http://www.risultati-ammissione.polimi.it/2023_20040_32ea_html/2023_20040_generale.html") };

        //print links found
        foreach (var r in rankingsUrls) Console.WriteLine($"[DEBUG] valid url found: {r.Url}");

        var outputJsonPath = Path.Join(baseFolder, Constants.OutputJsonFilename);

        var rankings = Parser.GetRankings(baseFolder, outputJsonPath, rankingsUrls);

        /*
        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        var rankingsSetFromHtmls = Parser.FindParseHtmls(baseFolder);

        //estraiamo i risultati dal web
        var rankingsSetFromWeb = Parser.ParseWeb(rankingsUrls);

        //estraiamo i risultati da un eventuale json locale
        var rankingsSetFromLocalJson = Parser.ParseLocalJson(outputJsonPath);

        //uniamo i dataset (quello dall'html, quello dal json locale, quello dal web)
        var fullRankingsSet = RankingsSet.Merge(rankingsSetFromHtmls, rankingsSetFromWeb, rankingsSetFromLocalJson);
        */

        //ottenere un json 
        var stringJson = JsonConvert.SerializeObject(rankings);

        //scriviamolo su disco
        File.WriteAllText(outputJsonPath, stringJson);

        //eliminare i suddetti file html
        /* if (transformerResult?.pathFound != null) */
        /*     FileUtils.TryBulkDelete(transformerResult.pathFound); */
        // ^^ this must be wrong
    }
}
