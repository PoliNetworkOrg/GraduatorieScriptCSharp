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

    public static RankingUrl From(string urlOrPath) {
        var r = new RankingUrl { url = urlOrPath, page = Page.Unknown };
        var url = urlOrPath.EndsWith(".html") ? urlOrPath.Remove(urlOrPath.Length - 5) : urlOrPath;

        if (url.EndsWith("generale")) r.page = Page.Index;
        else if (url.EndsWith("indice")) r.page = Page.IndexById;
        else if (url.EndsWith("indice_M")) r.page = Page.IndexByMerit;
        else if (url.EndsWith("sotto_indice")) r.page = Page.IndexByCourse;
        else 
        {
            var last = url.Split("/").Last();
            var splitByUnderscore = last.Split("_");
            var reversed = splitByUnderscore.Reverse().ToArray();

            if(reversed.First() == "M") r.page = Page.TableByMerit;
            else if(reversed[1] == "sotto") r.page = Page.TableByCourse;
            else if(reversed[1] == "grad") r.page = Page.TableById;
        }

        return r;
    }
}
