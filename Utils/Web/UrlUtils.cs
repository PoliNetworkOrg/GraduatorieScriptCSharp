using System.Net;

public class UrlUtils
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
        return href.StartsWith("/") ? domain + href : href;
    }
}
