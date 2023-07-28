#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;

#endregion

namespace PoliNetwork.Graduatorie.Common.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingUrl
{
    public PageEnum PageEnum = PageEnum.Unknown;
    public string Url = "";

    public override bool Equals(object? obj)
    {
        if (obj is not RankingUrl rankingUrl) return false;
        return PageEnum == rankingUrl.PageEnum && Url == rankingUrl.Url;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        var urlHash = Url.GetHashCode();
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return PageEnum.GetHashCode() ^ urlHash;
    }

    protected bool Equals(RankingUrl other)
    {
        return PageEnum == other.PageEnum && Url == other.Url;
    }


    public int GetHashWithoutLastUpdate()
    {
        var i = "RankingUrl".GetHashCode();
        i ^= Url.GetHashCode();
        i ^= PageEnum.GetHashCode();
        return i;
    }

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
        var fixedUrl = url.Replace("\\", "/");
        const string value = ".html";
        var cleanUrl = fixedUrl.EndsWith(value) ? fixedUrl.Remove(fixedUrl.Length - value.Length) : fixedUrl;
        return new RankingUrl { Url = url, PageEnum = GetPageEnum(cleanUrl) };
    }

    public string GetLocalPath(string htmlFolder)
    {
        var local = Url.Split("polimi.it/")[1];
        var fullPath = Path.Join(htmlFolder, local);

        var split = local.Split("/");
        var folder = Path.Join(htmlFolder, split[0]);
        if (Directory.Exists(folder))
            return fullPath;

        try
        {
            Directory.CreateDirectory(folder);
        }
        catch
        {
            Console.WriteLine("[ERROR] Can't create folder for html file: {folder}");
        }

        return fullPath;
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

    public string GetBaseDomain()
    {
        var lastUrlIndex = Url.LastIndexOf('/');
        var baseDomain = Url[..lastUrlIndex] + "/";
        return baseDomain;
    }

    public void FixSlashes()
    {
        Url = Url.Replace("\\", "/");
    }

    public bool IsSameRanking(RankingUrl urlB)
    {
        return AreSameRanking(this, urlB);
    }

    public bool AreSameRanking(RankingUrl urlA, RankingUrl urlB)
    {
        var a = urlA.Url;
        var b = urlB.Url;

        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            return true;

        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;

        a = a.Replace('\\', '/');
        b = b.Replace('\\', '/');

        if (!a.Contains('/') || !b.Contains('/'))
            return false;

        var splitA = a.Split("/");
        var splitB = b.Split("/");
        if (splitA.Length < 4 || splitB.Length < 4) return false;
        return splitA[3] == splitB[3];
    }

    public bool IsSimilar(RankingUrl urlB)
    {
        return AreSimilar(this, urlB);
    }

    public static bool AreSimilar(RankingUrl urlA, RankingUrl urlB)
    {
        var a = urlA.Url;
        var b = urlB.Url;

        if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            return true;

        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;

        a = a.Replace('\\', '/');
        b = b.Replace('\\', '/');

        if (!a.Contains('/') || !b.Contains('/'))
            return false;

        var aStrings = a.Split("/").Where(x => !string.IsNullOrEmpty(x) && x != "http:").ToList();
        var bStrings = b.Split("/").Where(x => !string.IsNullOrEmpty(x) && x != "http:").ToList();

        var min = Math.Min(aStrings.Count, bStrings.Count);
        aStrings = aStrings.Skip(Math.Max(0, aStrings.Count - min)).ToList();
        bStrings = bStrings.Skip(Math.Max(0, bStrings.Count - min)).ToList();

        for (var i = 0; i < min; i++)
            if (aStrings[i] != bStrings[i])
                return false;

        return true;
    }
}