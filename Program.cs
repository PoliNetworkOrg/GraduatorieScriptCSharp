// See https://aka.ms/new-console-template for more information

using GraduatorieScript;

Scraper scraper = new Scraper();
List<string> links = scraper.GetNewsLinks();
List<string> rankingsLinks = scraper.FindRankingsLink(links);

foreach (string link in rankingsLinks)
{
    Console.WriteLine(link);
}
