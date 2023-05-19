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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static HashSet<string> FindLinksFromCombinationsSingleYear(int i)
    {
        //http://www.risultati-ammissione.polimi.it/2022_20064_html/2022_20064_generale.html
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