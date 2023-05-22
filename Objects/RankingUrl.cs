namespace GraduatorieScript.Objects;

public enum Page
{
    Index,
    IndexById,
    IndexByMerit,
    IndexByCourse,
    TableById,
    TableByMerit,
    TableByCourse,
    Unknown
}

public class RankingUrl
{
    public Page page = Page.Unknown;
    public string url = "";

    /// <summary>
    ///     It creates a RankingUrl instance starting from the url
    /// </summary>
    /// <param name="url">
    ///     The full url string 
    ///     prefered input -> http://www.risultati-ammissione.polimi.it/2022_20064_html/2022_20064_generale.html
    ///     valid case -> /2022_20064_html/2022_20064_generale.html
    /// </param>
    /// <returns>RankingUrl</returns>
    public static RankingUrl From(string url) {
        var r = new RankingUrl { url = url, page = Page.Unknown };
        var cleanUrl = url.EndsWith(".html") ? url.Remove(url.Length - 5) : url;

        if (cleanUrl.EndsWith("generale")) r.page = Page.Index;
        else if (cleanUrl.EndsWith("indice")) r.page = Page.IndexById;
        else if (cleanUrl.EndsWith("indice_M")) r.page = Page.IndexByMerit;
        else if (cleanUrl.EndsWith("sotto_indice")) r.page = Page.IndexByCourse;
        else 
        {
            var last = cleanUrl.Split("/").Last();
            var splitByUnderscore = last.Split("_");
            var reversed = splitByUnderscore.Reverse().ToArray();

            if(reversed.First() == "M") r.page = Page.TableByMerit;
            else if(reversed[1] == "sotto") r.page = Page.TableByCourse;
            else if(reversed[1] == "grad") r.page = Page.TableById;
        }

        return r;
    }
}
