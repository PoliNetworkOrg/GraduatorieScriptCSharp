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
        const string value = ".html";
        var cleanUrl = url.EndsWith(value) ? url.Remove(url.Length - value.Length) : url;
        return new RankingUrl { url = url, PageEnum = GetPageEnum(cleanUrl) };
    }

    private static PageEnum GetPageEnum(string cleanUrl)
    {
        if (cleanUrl.EndsWith("generale"))
        {
            return PageEnum.Index;
        }

        if (cleanUrl.EndsWith("indice"))
        {
            return PageEnum.IndexById;
        }

        if (cleanUrl.EndsWith("indice_M"))
        {
            return PageEnum.IndexByMerit;
        }

        if (cleanUrl.EndsWith("sotto_indice"))
        {
            return PageEnum.IndexByCourse;
        }

        var last = cleanUrl.Split("/").Last();
        var splitByUnderscore = last.Split("_");
        var reversed = splitByUnderscore.Reverse().ToArray();

        if (reversed.First() == "M") return PageEnum.TableByMerit;
        if (reversed[1] == "sotto") return PageEnum.TableByCourse;
        if (reversed[1] == "grad") return PageEnum.TableById;

        return PageEnum.Unknown;
    }
}