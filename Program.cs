// See https://aka.ms/new-console-template for more information

using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Path;
using GraduatorieScript.Utils.Transformer;
using GraduatorieScript.Utils.Web;
using Newtonsoft.Json;

//costanti
const string jJson = "j.json";

//find links from web
var rankingsLinks = LinksFind.GetAll();

//print links found
foreach (var link in rankingsLinks) Console.WriteLine(link);

var baseFolder = PathUtil.FindDocsFolder();
Console.WriteLine($"{baseFolder} baseFolder");
var jJsonPath = baseFolder + "\\" + jJson;

//nella cartella trovata, leggere e analizzare gli eventuali file .html
var transformerResult = Parser.ParseHtmlFiles(baseFolder);

//estraiamo i risultati dal web
var rankingsSetFromWeb = Parser.ParseWeb(rankingsLinks);

//estraiamo i risultati da un eventuale json locale
var rankingsSetFromLocalJson = Parser.ParseLocalJson(jJsonPath);

//uniamo i dataset (quello dall'html, quello dal json locale, quello dal web)
var rankingsSets = new List<RankingsSet?>
    { transformerResult?.RankingsSet, rankingsSetFromWeb, rankingsSetFromLocalJson };
var rankingsSet = RankingsSet.Merge(rankingsSets);

//ottenere un json 
var stringJson = JsonConvert.SerializeObject(rankingsSet);

//scriviamolo su disco
File.WriteAllText(jJsonPath, stringJson);

//eliminare i suddetti file html
FileUtil.DeleteFiles(transformerResult?.pathFound);