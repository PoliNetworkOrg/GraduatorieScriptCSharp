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
    
    public static void WriteAllIndexes(RankingsSet rankingsSet, string outFolder, ArgsConfig argsConfig)
    {
        //now let's write each single different index
        BySchoolYearJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearJson.CustomPath);
        ByYearSchoolJson.From(rankingsSet)?.WriteToFile(outFolder, ByYearSchoolJson.CustomPath);
        BySchoolYearCourseJson.From(rankingsSet)?.WriteToFile(outFolder, BySchoolYearCourseJson.CustomPath);
    }
}