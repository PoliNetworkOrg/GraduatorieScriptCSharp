﻿#region

using PoliNetwork.Core.Utils;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Extensions;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public static class LinksFind
{
    public static IEnumerable<RankingUrl> GetAll()
    {
        var mt = new Metrics();
        var polimiNewsLinks = mt.Execute(GetPolimiNewsLink);
        var combinationLinks = mt.Execute(GetCombinationLinks);

        //merge results links
        var rankingsLinks = new HashSet<string>();
        rankingsLinks.AddRange(polimiNewsLinks, combinationLinks);

        var rankingsUrls = CheckUrlUtil.GetRankingLinks(rankingsLinks);

        var len = rankingsUrls.ToArray().Length;
        Console.WriteLine($"[INFO] LinksFind.GetAll found {len} links");
        return rankingsUrls;
    }


    private static IEnumerable<string> GetCombinationLinks()
    {
        var r = new HashSet<string>();
        var nowYear = DateTime.UtcNow.Year;
        for (var i = 2021; i <= nowYear; i++) r.AddRange(GetYearCombinationLinks(i));
        return r;
    }

    private static IEnumerable<string> GetYearCombinationLinks(int year)
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
        var r = new HashSet<string>();
        r.AddRange(links);
        return r;
    }

    private static IEnumerable<string> GetPolimiNewsLink()
    {
        //scrape links from polimi news
        var scraper = new Scraper();
        var findRankingsLink = scraper.GetRankingsLinks();
        return findRankingsLink;
    }
}