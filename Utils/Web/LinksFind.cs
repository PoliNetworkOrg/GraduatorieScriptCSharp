namespace GraduatorieScript.Utils.Web;

public static class LinksFind
{
    public static HashSet<string?> FindLinksFromWeb()
    {
        var findRankingsLink = FindLinksFromPolimiNews();
        var linksFromCombinations = FindLinksFromCombinations();

        //merge results links
        var listOfListOfLinks = new List<List<string>> { findRankingsLink , linksFromCombinations};
        var rankingsLinks = Strings.StringUtil.Merge(listOfListOfLinks);
        return rankingsLinks;
    }

    private static List<string> FindLinksFromCombinations()
    {
        throw new NotImplementedException();
    }

    private static List<string> FindLinksFromPolimiNews()
    {
        //scrape links from polimi news
        var scraper = new Scraper();
        var links = scraper.GetNewsLinks();
        var findRankingsLink = scraper.FindRankingsLink(links);
        return findRankingsLink;
    }
}