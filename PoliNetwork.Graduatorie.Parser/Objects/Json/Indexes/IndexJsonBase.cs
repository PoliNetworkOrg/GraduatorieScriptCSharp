#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public abstract class IndexJsonBase
{
    public DateTime? LastUpdate;

    public void WriteToFile(string outFolder, string pathFile)
    {
        var mainJsonPath = Path.Join(outFolder, pathFile);
        var mainJsonString = JsonConvert.SerializeObject(this, Culture.JsonSerializerSettings);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }


    public static void WriteSingleJsons(RankingsSet? set, string outFolder, ArgsConfig argsConfig)
    {
        if (set == null)
            return;

        // group rankings by year
        var bySchool = set.Rankings.GroupBy(r => r.School);
        foreach (var schoolGroup in bySchool)
        {
            if (schoolGroup.Key is null)
                continue;
            var school = schoolGroup.Key.Value;

            var byYears = schoolGroup.GroupBy(r => r.Year);
            foreach (var yearGroup in byYears)
            {
                if (yearGroup.Key is null)
                    continue;
                var year = yearGroup.Key.Value;
                var folder = Path.Join(outFolder, school.ToString(), year.ToString());
                Directory.CreateDirectory(folder);

                foreach (var ranking in yearGroup) WriteSingleJsonRanking(folder, ranking, argsConfig);
            }
        }
    }

    private static void WriteSingleJsonRanking(string folder, Ranking ranking, ArgsConfig argsConfig)
    {
        var path = Path.Join(folder, ranking.ConvertPhaseToFilename());

        if (ExitIfAlreadyExistsAndNotUpdated(ranking, path) && !argsConfig.ForceReparsing) return;

        var rankingJsonString = JsonConvert.SerializeObject(ranking, Culture.JsonSerializerSettings);
        File.WriteAllText(path, rankingJsonString);
    }

    private static bool ExitIfAlreadyExistsAndNotUpdated(Ranking ranking, string path)
    {
        if (!File.Exists(path)) return false;

        var j = GetRankingFromFile(path);
        if (j == null)
            return false;
        var hashThis = ranking.GetHashWithoutLastUpdate();
        var hashJ = j?.GetHashWithoutLastUpdate();
        return hashThis == hashJ;
    }

    private static Ranking? GetRankingFromFile(string path)
    {
        var x = File.ReadAllText(path);

        var j = JsonConvert.DeserializeObject<Ranking>(x, Culture.JsonSerializerSettings);
        return j;
    }

    public static void IndexesWrite(RankingsSet? rankingsSet, string outFolder, ArgsConfig argsConfig)
    {
        //let's write all single json files
        WriteSingleJsons(rankingsSet, outFolder, argsConfig);

        //now let's write each single different index
        BySchoolYearJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearJson.PathCustom);
        ByYearSchoolJson.From(rankingsSet)?.WriteToFile(outFolder, ByYearSchoolJson.PathCustom);
        BySchoolYearCourseJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearCourseJson.PathCustom);
    }
}