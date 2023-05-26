using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Extensions;
using GraduatorieScript.Objects;
using GraduatorieScript.Utils.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GraduatorieScript.Utils.Transformer;

public static class Parser
{
    public static RankingsSet? FindParseHtmls(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        //nella cartella trovata, leggere e analizzare gli eventuali file .html
        var files = Directory.GetFiles(path, "*.html", SearchOption.AllDirectories);
        var rankingsSet = new RankingsSet { LastUpdate = DateTime.Now };
        foreach (var file in files)
        {
            var fileRelativePath = file.Split(path)[1];

            // ignore because this is the file built 
            // by previous script which is useless for this one
            // (and it also breaks our logic)
            if (fileRelativePath == "index.html") continue;

            var html = File.ReadAllText(file);
            var url = $"http://{Constants.RisultatiAmmissionePolimiIt}{fileRelativePath}";
            // no need to check if url is online
            // because the html is already stored


            var ranking = ParseHtml(html, RankingUrl.From(url));
            if (ranking != null) rankingsSet.AddRanking(ranking);
        }

        return rankingsSet;
    }

    public static Ranking? ParseHtml(string html, RankingUrl url)
    {
        if (string.IsNullOrEmpty(html) || url.page == Page.Unknown) return null;

        //todo: da un testo formattato in html, ottenere la graduatoria o ogni altra informazione 
        //e aggiungerla alla classe attuale, evitando ripetizioni

        var page = new HtmlDocument();
        page.LoadHtml(html);
        var doc = page.DocumentNode;

        var intestazione = doc
            .GetElementsByClassName("intestazione")
            .Select(el => el.InnerText)
            .First(text => text.Contains("Politecnico"));

        if (string.IsNullOrEmpty(intestazione)) return null;

        Console.WriteLine($"{url.url} {url.page} valid");

        return null;
        //TODO: throw new NotImplementedException(); // just as a reminder
    }

    public static RankingsSet ParseWeb(IEnumerable<RankingUrl> rankingsUrls)
    {
        //download delle graduatorie, ricorsivamente, e inserimento nel rankingsSet
        var rankingsSet = new RankingsSet
        {
            LastUpdate = DateTime.Now,
            Rankings = new List<Ranking>()
        };

        foreach (var r in rankingsUrls)
        {
            var download = Scraper.Download(r.url);
            if (download != null) rankingsSet.Rankings.Add(download);
        }

        return rankingsSet;
    }

    public static RankingsSet? ParseLocalJson(string jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            return null;

        var fileContent = File.ReadAllText(jsonPath);
        if (string.IsNullOrEmpty(fileContent))
            return null;

        var rankingsSet = JsonConvert.DeserializeObject<RankingsSet>(fileContent);
        return rankingsSet;
    }
}