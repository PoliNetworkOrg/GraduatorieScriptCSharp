using GraduatorieScript.Enums;

namespace GraduatorieScript.Objects;

public class RankingUrl
{
    public PageEnum PageEnum = PageEnum.Unknown;
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
    public static RankingUrl From(string url)
    {
        var r = new RankingUrl { url = url, PageEnum = PageEnum.Unknown };
        var cleanUrl = url.EndsWith(".html") ? url.Remove(url.Length - 5) : url;

        if (cleanUrl.EndsWith("generale"))
        {
            r.PageEnum = PageEnum.Index;
        }
        else if (cleanUrl.EndsWith("indice"))
        {
            r.PageEnum = PageEnum.IndexById;
        }
        else if (cleanUrl.EndsWith("indice_M"))
        {
            r.PageEnum = PageEnum.IndexByMerit;
        }
        else if (cleanUrl.EndsWith("sotto_indice"))
        {
            r.PageEnum = PageEnum.IndexByCourse;
        }
        else
        {
            var last = cleanUrl.Split("/").Last();
            var splitByUnderscore = last.Split("_");
            var reversed = splitByUnderscore.Reverse().ToArray();

            if (reversed.First() == "M") r.PageEnum = PageEnum.TableByMerit;
            else if (reversed[1] == "sotto") r.PageEnum = PageEnum.TableByCourse;
            else if (reversed[1] == "grad") r.PageEnum = PageEnum.TableById;
        }

        return r;
    }
}