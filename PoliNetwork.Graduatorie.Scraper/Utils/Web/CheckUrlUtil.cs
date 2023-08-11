#region

using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public class CheckUrlUtil
{
    public static void CheckUrl(RankingUrl variable, HashSet<RankingUrl> final)
    {
        try
        {
            var x = UrlUtils.CheckUrl(variable);
            if (!x) return;
            lock (final)
            {
                final.Add(variable);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public static HashSet<RankingUrl> GetRankingLinks(IEnumerable<string> rankingsLinks)
    {
        var parallelQuery = rankingsLinks
            .AsParallel()
            .Select(RankingUrl.From)
            .Where(r => r.PageEnum == PageEnum.Index).ToList();

        return GetRankingLinksHashSet(parallelQuery);
    }

    public static HashSet<RankingUrl> GetRankingLinksHashSet(IEnumerable<RankingUrl> parallelQuery)
    {
        var final = new HashSet<RankingUrl>();

        var action = parallelQuery.Select((Func<RankingUrl, Action>)Selector).ToArray();
        Parallel.Invoke(action);

        return final;

        Action Selector(RankingUrl variable)
        {
            return () => { CheckUrl(variable, final); };
        }
    }
}