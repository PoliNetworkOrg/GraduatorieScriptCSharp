#region

using HtmlAgilityPack;

#endregion

namespace PoliNetwork.Graduatorie.Common.Extensions;

public static class HtmlNodeExtensions
{
    public static IEnumerable<HtmlNode> GetElementsByName(this HtmlNode parent, string name)
    {
        return parent.Descendants().Where(node => node.Name == name);
    }

    public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode parent, string name)
    {
        return parent.Descendants(name);
    }

    public static IEnumerable<HtmlNode> GetElementsByClassName(this HtmlNode parent, string className)
    {
        return parent.Descendants().Where(node => node.HasClass(className));
    }
}