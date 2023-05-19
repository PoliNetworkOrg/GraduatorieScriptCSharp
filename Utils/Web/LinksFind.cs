namespace GraduatorieScript.Utils.Web;

public static class LinksFind
{
    public static HashSet<string?> FindLinksFromWeb()
    {
        var findRankingsLink = FindLinksFromPolimiNews();
        var linksFromCombinations = FindLinksFromCombinations();

        //merge results links
        var listOfListOfLinks = new List<HashSet<string>> { findRankingsLink , linksFromCombinations};
        var rankingsLinks = Strings.StringUtil.Merge(listOfListOfLinks);
        return rankingsLinks;
    }

    private static HashSet<string> FindLinksFromCombinations()
    {
        HashSet<string> r = new HashSet<string>();
        for (int i = DateTime.Now.Year - 1; i <= DateTime.Now.Year; i++)
        {
            HashSet<string> r2 = FindLinksFromCombinationsSingleYear(i);
            foreach (var VARIABLE in r2)
            {
                r.Add(VARIABLE);
            }
        }

        return r;
    }

    private static HashSet<string> FindLinksFromCombinationsSingleYear(int i)
    {
        throw new NotImplementedException();
    }

    private static HashSet<string> FindLinksFromPolimiNews()
    {
        //scrape links from polimi news
        var scraper = new Scraper();
        var links = scraper.GetNewsLinks();
        var findRankingsLink = scraper.FindRankingsLink(links);
        return findRankingsLink;
    }
}