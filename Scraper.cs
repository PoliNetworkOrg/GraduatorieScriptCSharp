using HtmlAgilityPack;

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
}

namespace GraduatorieScript
{
    struct AnchorElement
    {
        public string Name;
        public string Url;
    }

    public class Scraper
    {
        HtmlWeb web = new HtmlWeb();
        string NewsUrl = "https://www.polimi.it/in-evidenza";
        string[] NewsTesters = {
            "graduatorie", "graduatoria", "punteggi", "tol",
            "immatricolazioni", "immatricolazione", "punteggio",
            "matricola", "merito", "nuovi studenti" };

        static string UrlifyLocalHref(string href) {
            return href.StartsWith("/") ? "https://polimi.it" + href : href;
        }

        public List<string> GetNewsLinks()
        {
            HtmlDocument htmlDoc = web.Load(NewsUrl);

            List<AnchorElement> AnchorElements = htmlDoc.DocumentNode
                .SelectNodes("//*[@id=\"c42275\"]/ul/li/h3/a")
                .Select(element =>
                {
                    string href = element.Attributes["href"].Value;
                    string url = UrlifyLocalHref(href);
                    return new AnchorElement { Name = element.InnerText, Url = url };
                })
                .ToList();

            List<string> FilteredLinks = AnchorElements
                /* .Where(anchor => NewsTesters.Contains(anchor.Name.ToLower())) */
                .Select(anchor => anchor.Url)
                .ToList();

            return FilteredLinks;
        }

        public List<string> FindRankingsLink(List<string> NewsLink)
        {
            List<string> RankingsList = new List<string>();
            List<Action> checkNewsLinks = new List<Action>();

            foreach (string Link in NewsLink)
            {
                string currentLink = Link; // Create a local variable to capture the correct value in the lambda expression
                checkNewsLinks.Add(() =>
                {
                    HtmlDocument htmlDoc = web.Load(NewsUrl);
                    List<string> links = htmlDoc.DocumentNode
                    .GetElementsByTagName("a")
                    .Select(element => UrlifyLocalHref(element.GetAttributeValue("href", string.Empty)))
                    .Where(url => url.Contains("risultati-ammissione.polimi.it"))
                    .ToList();

                    lock (RankingsList) {
                        RankingsList.AddRange(links);
                    }
                });
            }

            Parallel.Invoke(checkNewsLinks.ToArray());
            return RankingsList.Distinct().ToList();
        }
    }
}
