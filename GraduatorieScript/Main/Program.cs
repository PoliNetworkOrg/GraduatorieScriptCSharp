using GraduatorieScript.Data;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;
using SampleNuGet.Utils;

namespace GraduatorieScript.Main;

public static class Program
{
    public static void Main(string[] args)
    {
        var mt = new Metrics();

        var docsFolder = args.Length > 0 && !string.IsNullOrEmpty(args[0])
            ? args[0]
            : PathUtils.FindFolder(Constants.FolderToFind);
        Console.WriteLine($"[INFO] baseFolder [1]: {docsFolder}");

        if (string.IsNullOrEmpty(docsFolder))
            docsFolder = PathUtils.CreateAndReturnDocsFolder(Constants.FolderToFind);
        Console.WriteLine($"[INFO] baseFolder [2]: {docsFolder}");

        if (string.IsNullOrEmpty(docsFolder))
        {
            Console.WriteLine("[INFO] baseFolder is null. Abort.");
            return;
        }

        //find links from web
        var rankingsUrls = mt.Execute(LinksFind.GetAll).ToList();
        ScraperOutput.Write(rankingsUrls, docsFolder);

        //print links found
        foreach (var r in rankingsUrls) 
            Console.WriteLine($"[DEBUG] valid url found: {r.Url}");

        var outputJsonPath = Path.Join(docsFolder, Constants.OutputJsonFilename);

        // ricava un unico set partendo dai file html salvati, dagli url trovati e
        // dal precedente set salvato nel .json
        var rankingsSet = Parser.GetRankings(docsFolder, outputJsonPath, rankingsUrls);

        //ottenere un json 
        var stringJson = JsonConvert.SerializeObject(rankingsSet, Formatting.Indented);

        //scriviamolo su disco
        File.WriteAllText(outputJsonPath, stringJson);

        //eliminare i suddetti file html
        /* if (transformerResult?.pathFound != null) */
        /*     FileUtils.TryBulkDelete(transformerResult.pathFound); */
        // ^^ this must be wrong
    }
}
