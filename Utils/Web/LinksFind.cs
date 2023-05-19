namespace GraduatorieScript.Utils.Web;

public static class LinksFind
{
    public static HashSet<string?> FindLinksFromWeb()
    {
        //scrape links from polimi news
        var scraper = new Scraper();
        var links = scraper.GetNewsLinks();
        var findRankingsLink = scraper.FindRankingsLink(links);

        //merge results links
        var listOfListOfLinks = new List<List<string>> { findRankingsLink };
        var rankingsLinks = GraduatorieScript.Utils.Strings.StringUtil.Merge(listOfListOfLinks);
        return rankingsLinks;
    }
}