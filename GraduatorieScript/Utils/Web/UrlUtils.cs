using System.Net;

namespace GraduatorieScript.Utils.Web;

public static class UrlUtils
{
    /// <summary>
    ///     Taken an href from the a tag which could be either an internal link or an
    ///     external one, this method returns the full url using the given domain
    /// </summary>
    /// <param name="href">The href from the html anchor tag taken from the news content.</param>
    /// <param name="domain">https://example.com</param>
    /// <returns>The full url</returns>
    public static string UrlifyLocalHref(string href, string domain)
    {
        return !href.Contains(domain) ? domain + href : href;
    }

    public static bool CheckUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        using var client = new HttpClient();
        try
        {
            var response = client.GetAsync(url).Result;
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}