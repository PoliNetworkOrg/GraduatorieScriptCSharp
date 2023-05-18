// See https://aka.ms/new-console-template for more information

using GraduatorieScript.Utils;
using GraduatorieScript.Utils.Web;

var scraper = new Scraper();
var links = scraper.GetNewsLinks();
var rankingsLinks = scraper.FindRankingsLink(links);

foreach (var link in rankingsLinks) Console.WriteLine(link);