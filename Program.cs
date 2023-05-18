﻿// See https://aka.ms/new-console-template for more information

using GraduatorieScript.Objects;
using GraduatorieScript.Utils;
using GraduatorieScript.Utils.Web;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;

var scraper = new Scraper();
var links = scraper.GetNewsLinks();
var rankingsLinks = scraper.FindRankingsLink(links);

foreach (var link in rankingsLinks) Console.WriteLine(link);

var baseFolder = GraduatorieScript.Utils.Path.PathUtil.FindDocsFolder();
Console.WriteLine($"{baseFolder} baseFolder");
var jJsonPath = baseFolder + "\\" + "j.json";

//nella cartella trovata, leggere e analizzare gli eventuali file .html
TransformerResult transformerResult = GraduatorieScript.Utils.Transformer.Parser.ParseHtmlFiles(baseFolder);

RankingsSet rankingsSetFromWeb = GraduatorieScript.Utils.Transformer.Parser.ParseWeb(rankingsLinks);
RankingsSet? rankingsSetFromLocalJson = GraduatorieScript.Utils.Transformer.Parser.ParseLocalJson(jJsonPath);

//uniamo i dataset (quello dall'html, quello dal json locale, quello dal web)
var rankingsSet = RankingsSet.Merge( new List<RankingsSet?> (){ transformerResult.RankingsSet, rankingsSetFromWeb, rankingsSetFromLocalJson});

//ottenere un json 
var stringJson = Newtonsoft.Json.JsonConvert.SerializeObject(rankingsSet);

//scriviamolo su disco

File.WriteAllText(jJsonPath, stringJson);

//eliminare i suddetti file html
foreach (var toDelete in transformerResult.pathFound)
{
    GraduatorieScript.Utils.Path.FileUtil.TryDelete(toDelete);
}

