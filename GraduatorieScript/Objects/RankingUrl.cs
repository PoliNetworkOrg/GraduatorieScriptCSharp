using GraduatorieScript.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingUrl
{
    public PageEnum PageEnum = PageEnum.Unknown;
    public string Url = "";

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
        return new RankingUrl { Url = url, PageEnum = GetPageEnum(cleanUrl) };
    }

    private static PageEnum GetPageEnum(string cleanUrl)
    {
        // Console.WriteLine($"[DEBUG] calculate PageEnum of {cleanUrl}");
        if (cleanUrl.EndsWith("generale")) return PageEnum.Index;

        if (cleanUrl.EndsWith("sotto_indice")) return PageEnum.IndexByCourse;

        if (!cleanUrl.EndsWith("sotto_indice") && cleanUrl.EndsWith("indice")) return PageEnum.IndexById;

        if (cleanUrl.EndsWith("indice_M")) return PageEnum.IndexByMerit;


        var last = cleanUrl.Split("/").Last();
        var splitByUnderscore = last.Split("_");
        var reversed = splitByUnderscore.Reverse().ToArray();

        return TableByCourse(reversed);
    }

    private static PageEnum TableByCourse(IReadOnlyList<string> reversed)
    {
        if (reversed[0] == "M") return PageEnum.TableByMerit;
        return reversed[1] switch
        {
            "sotto" => PageEnum.TableByCourse,
            "grad" => PageEnum.TableById,
            _ => PageEnum.Unknown
        };
    }
}
