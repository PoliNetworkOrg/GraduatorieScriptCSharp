using GraduatorieScript.Data;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.Json.Indexes.Specific;
using GraduatorieScript.Objects.RankingNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Indexes;

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


    public static void WriteSingleJsons(RankingsSet? set, string outFolder)
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

                foreach (var ranking in yearGroup) WriteSingleJsonRanking(folder, ranking);
            }
        }
    }

    private static void WriteSingleJsonRanking(string folder, Ranking ranking)
    {
        var path = Path.Join(folder, ranking.ConvertPhaseToFilename());

        if (ExitIfAlreadyExistsAndNotUpdated(ranking, path)) return;

        var rankingJsonString = JsonConvert.SerializeObject(ranking, Culture.JsonSerializerSettings);
        File.WriteAllText(path, rankingJsonString);
    }

    private static bool ExitIfAlreadyExistsAndNotUpdated(Ranking ranking, string path)
    {
        if (!File.Exists(path)) return false;

        var x = File.ReadAllText(path);

        var j = JsonConvert.DeserializeObject<Ranking>(x, Culture.JsonSerializerSettings);
        var hashThis = ranking.GetHashWithoutLastUpdate();
        var hashJ = j?.GetHashWithoutLastUpdate();
        return hashThis == hashJ;
    }

    public static void IndexesWrite(RankingsSet? rankingsSet, string outFolder)
    {
        //let's write all single json files
        WriteSingleJsons(rankingsSet, outFolder);

        //now let's write each single different index
        BySchoolYearJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearJson.PathCustom);
        ByYearSchoolJson.From(rankingsSet)?.WriteToFile(outFolder, ByYearSchoolJson.PathCustom);
        BySchoolYearCourseJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearCourseJson.PathCustom);
    }
}