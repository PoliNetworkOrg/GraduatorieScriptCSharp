// See https://aka.ms/new-console-template for more information

using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;

var scraper = new Scraper();
var links = scraper.GetNewsLinks();
var enumerable = new List<List<string>> () { scraper.FindRankingsLink(links) };
var rankingsLinks = GraduatorieScript.Utils.Strings.StringUtil.Merge(enumerable);

foreach (var link in rankingsLinks) Console.WriteLine(link);

var baseFolder = GraduatorieScript.Utils.Path.PathUtil.FindDocsFolder();
Console.WriteLine($"{baseFolder} baseFolder");
var jJsonPath = baseFolder + "\\" + "j.json";

//nella cartella trovata, leggere e analizzare gli eventuali file .html
var transformerResult = GraduatorieScript.Utils.Transformer.Parser.ParseHtmlFiles(baseFolder);

//estraiamo i risultati dal web
var rankingsSetFromWeb = GraduatorieScript.Utils.Transformer.Parser.ParseWeb(rankingsLinks);

//estraiamo i risultati da un eventuale json locale
var rankingsSetFromLocalJson = GraduatorieScript.Utils.Transformer.Parser.ParseLocalJson(jJsonPath);

//uniamo i dataset (quello dall'html, quello dal json locale, quello dal web)
var rankingsSets = new List<RankingsSet?> { transformerResult.RankingsSet, rankingsSetFromWeb, rankingsSetFromLocalJson};
var rankingsSet = RankingsSet.Merge( rankingsSets);

//ottenere un json 
var stringJson = Newtonsoft.Json.JsonConvert.SerializeObject(rankingsSet);

//scriviamolo su disco
File.WriteAllText(jJsonPath, stringJson);

//eliminare i suddetti file html
if (transformerResult.pathFound != null)
    foreach (var toDelete in transformerResult.pathFound)
    {
        GraduatorieScript.Utils.Path.FileUtil.TryDelete(toDelete);
    }

