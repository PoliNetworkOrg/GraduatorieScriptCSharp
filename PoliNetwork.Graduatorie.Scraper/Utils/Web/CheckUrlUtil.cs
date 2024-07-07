#region

using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public static class CheckUrlUtil
{
    public static HashSet<RankingUrl> GetRankingLinks(IEnumerable<string> rankingsLinks)
    {
        var parallelQuery = rankingsLinks
            .AsParallel()
            .Select(RankingUrl.From)
            .Where(r => r.PageEnum == PageEnum.Index).ToList();

        return GetRankingLinksHashSet(parallelQuery);
    }

    public static HashSet<RankingUrl> GetRankingLinksHashSet(IEnumerable<RankingUrl> urls)
    {
        var hashSet = new HashSet<RankingUrl>();

        var actions = urls.Select((Func<RankingUrl, Action>)Selector).ToArray();
        Parallel.Invoke(actions);

        return hashSet;

        Action Selector(RankingUrl url)
        {
            return () => { CheckUrl(url, hashSet); };
        }
    }

    private static void CheckUrl(RankingUrl url, HashSet<RankingUrl> hashSet)
    {
        try
        {
            var x = UrlUtils.CheckUrl(url);
            if (!x) return;
            lock (hashSet)
            {
                hashSet.Add(url);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}