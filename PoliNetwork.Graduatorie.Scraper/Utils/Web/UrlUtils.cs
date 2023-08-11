#region

using System.Net;
using PoliNetwork.Graduatorie.Common.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

public static class UrlUtils
{
    /// <summary>
    ///     Taken an href from the a tag which could be either an internal link or an
    ///     external one, this method returns the full url using the given domain
    /// </summary>
    /// <param name="href">The href from the html anchor tag taken from the news content.</param>
    /// <param name="domain">https://example.com</param>
    /// <returns>The full url</returns>
    public static string UrlifyLocalHref(string href, string? domain)
    {
        return domain != null && !href.Contains(domain) ? domain + href : href;
    }

    public static bool CheckUrl(RankingUrl? url)
    {
        var urlUrl = url?.Url;
        if (string.IsNullOrEmpty(urlUrl))
            return false;

        using var client = new HttpClient();
        try
        {
            var async = client.GetAsync(urlUrl);
            var response = async.Result;
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}