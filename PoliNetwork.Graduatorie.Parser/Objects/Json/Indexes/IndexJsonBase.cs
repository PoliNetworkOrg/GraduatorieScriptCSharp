#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

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

    private static bool ExitIfAlreadyExistsAndNotUpdated(Ranking a, string path)
    {
        if (!File.Exists(path)) return false;
        var b = GetRankingFromFile(path);
        return b != null && SameHash(a, b);
    }

    private static bool SameHash(Ranking a, Ranking b)
    {
        var aInfo = a.GetHashWithoutLastUpdateInfo();
        var bInfo = b.GetHashWithoutLastUpdateInfo();
        var aTableCourse = a.GetTableCourse();
        var bTableCourse = b.GetTableCourse();
        var aTableMerit = a.GetMerit();
        var bTableMerit = b.GetMerit();
        
        var sameHashInfo = aInfo == bInfo;
        var sameHashTableCourse = SameHashCourse(aTableCourse, bTableCourse);
        var sameHashTableMerit = SameHashMerit(aTableMerit, bTableMerit);
        return sameHashInfo && sameHashTableCourse && sameHashTableMerit;
    }

    private static bool SameHashCourse(IReadOnlyCollection<CourseTable>? aTableCourse, IReadOnlyCollection<CourseTable>? bTableCourse)
    {
        if (aTableCourse == null && bTableCourse == null)
            return true;
        if (aTableCourse == null || bTableCourse == null)
            return false;

        ;

        if (aTableCourse.Count != bTableCourse.Count)
            return false;
        ;

        var aHash = aTableCourse.Select(variable =>
        {
            var hashWithoutLastUpdate = Ranking.GetHashFromListHash(variable.GetHashWithoutLastUpdate());
            return hashWithoutLastUpdate;
        }).ToList();

        var bHash = bTableCourse.Select(variable =>
        {
            var hashWithoutLastUpdate = Ranking.GetHashFromListHash(variable.GetHashWithoutLastUpdate());
            return hashWithoutLastUpdate;
        }).ToList();

        var ai = Ranking.GetHashFromListHash(aHash);
        var bi = Ranking.GetHashFromListHash(bHash);
        
        return (ai ?? 0) == (bi ?? 0);
    }

    private static bool SameHashMerit(MeritTable? aTableMerit, MeritTable? bTableMerit)
    {
        if (aTableMerit == null && bTableMerit == null)
            return true;
        if (aTableMerit == null || bTableMerit == null)
            return false;

        ;
        var ai = aTableMerit.GetHashWithoutLastUpdate();
        var bi = bTableMerit.GetHashWithoutLastUpdate();
        var aii = Ranking.GetHashFromListHash(ai) ?? 0;
        var bii = Ranking.GetHashFromListHash(bi) ?? 0;
        return aii == bii;
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