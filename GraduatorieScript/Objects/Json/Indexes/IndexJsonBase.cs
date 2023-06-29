using GraduatorieScript.Data.Constants;
using GraduatorieScript.Objects.Json.Indexes.Specific;
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
        var mainJsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(mainJsonPath, mainJsonString);
    }


    public static void WriteSingleJsons(RankingsSet set, string outFolder)
    {
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

                foreach (var ranking in yearGroup)
                {
                    var path = Path.Join(folder, ranking.ConvertPhaseToFilename());
                    var rankingJsonString = JsonConvert.SerializeObject(ranking, Formatting.Indented);
                    File.WriteAllText(path, rankingJsonString);
                }
            }
        }
    }

    public static void IndexesWrite(RankingsSet rankingsSet, string outFolder)
    {
        //let's write all single json files
        WriteSingleJsons(rankingsSet, outFolder);

        //now let's write each single different index
        BySchoolYearJson.From(rankingsSet).WriteToFile(outFolder, IndexesPathConstants.BySchoolYearFilename);
    }
}