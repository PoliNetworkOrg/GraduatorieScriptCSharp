using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;
using HtmlAgilityPack;

namespace GraduatorieScript.Utils.Transformer;

public class HtmlPage
{
    public readonly HtmlDocument Html;
    private readonly string HtmlString;
    public readonly RankingUrl Url;

    public HtmlPage(string html, RankingUrl url)
    {
        var page = new HtmlDocument();
        page.LoadHtml(html);
        Html = page;
        HtmlString = html;
        Url = url;
    }

    new public string ToString() {
        return HtmlString;
    }

    public static HtmlPage? FromUrl(RankingUrl url)
    {
        var html = Scraper.Download(url.Url);
        if (html is null || string.IsNullOrEmpty(html))
            return null;
        return new HtmlPage(html, url);
    }
}
