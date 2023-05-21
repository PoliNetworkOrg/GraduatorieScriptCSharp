using GraduatorieScript.Data;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Strings;

namespace GraduatorieScript.Utils.Web;

public static class LinksFind
{
    public static HashSet<string> GetAll()
    {
        var polimiNewsLinks = GetPolimiNewsLink();
        var combinationLinks = GetCombinationLinks();

        //merge results links
        var joinedList = new List<HashSetExtended<string>> { polimiNewsLinks, combinationLinks };
        var rankingsLinks = StringUtil.Merge(joinedList);
        return rankingsLinks;
    }

    private static HashSetExtended<string> GetCombinationLinks()
    {
        var r = new HashSetExtended<string>();
        for (var i = DateTime.Now.Year - 1; i <= DateTime.Now.Year; i++) r.AddRange(GetYearCominationLinks(i));
        return r;
    }

    private static HashSetExtended<string> GetYearCominationLinks(int year)
    {
        // partial implemented: polimi has recently added 4 hex chars in the first part 
        // of the path (2022_20064_XXXX_html/) which would require 65k combinations for each 
        // key (2, 5, 6, ...), so 1.1 million links to check 
        // TBD whether or not to implement it 
        // 
        // The following code only create combinations with the old method
        int[] keys = { 2, 5, 6, 7, 8, 40, 41, 42, 45, 54, 60, 64, 69, 91, 102, 103, 104 };
        var ids = keys.Select(k => $"20{k:D3}").ToArray(); // 20002, 20005, ...
        var yearIds = ids.Select(i => $"{year}_{i}").ToArray(); // 2022_20002, 2022_20005, ...
        var links = yearIds
            //http://www.risultati-ammissione.polimi.it/2022_20064_html/2022_20064_generale.html
            .Select(id => $"http://{Constants.RisultatiAmmissionePolimiIt}/{id}_html/{id}_generale.html")
            .ToHashSet();
        var r = new HashSetExtended<string>();
        r.AddRange(links);
        return r;
    }

    private static HashSetExtended<string> GetPolimiNewsLink()
    {
        //scrape links from polimi news
        var scraper = new Scraper();
        var newsLinks = scraper.GetNewsLinks();
        var findRankingsLink = scraper.FindRankingsLink(newsLinks);
        return findRankingsLink;
    }
}