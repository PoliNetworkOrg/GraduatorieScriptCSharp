using GraduatorieScript.Utils.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class HtmlPage
{
    private readonly string _htmlString;
    public readonly HtmlDocument Html;
    public readonly RankingUrl Url;

    public HtmlPage(string html, RankingUrl url)
    {
        var page = new HtmlDocument();
        page.LoadHtml(html);
        Html = page;
        _htmlString = html;
        Url = url;
    }

    public override string ToString()
    {
        return _htmlString;
    }

    public static HtmlPage? FromUrl(RankingUrl url)
    {
        var html = Scraper.Download(url.Url);
        if (html is null || string.IsNullOrEmpty(html))
            return null;
        return new HtmlPage(html, url);
    }
}