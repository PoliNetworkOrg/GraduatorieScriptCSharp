using GraduatorieScript.Objects.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingSummary
{
    public List<CourseTableStats>? CourseSummarized;
    public int? HowManyCanEnroll;
    public int? HowManyStudents;
    public Dictionary<int, int>? ResultsSummarized; //key=score, value=howManyGotThatScore

    public int GetHashWithoutLastUpdate()
    {
        var i = (HowManyStudents ?? 0) ^ (HowManyCanEnroll ?? 0);
        if (CourseSummarized != null)
            foreach (var variable in CourseSummarized)
                i ^= variable.GetHashWithoutLastUpdate();

        if (ResultsSummarized != null)
            foreach (var variable in ResultsSummarized)
                i ^= variable.Key ^ variable.Value;

        return i;
    }

    public static RankingSummary From(Ranking ranking)
    {
        var byMeritRows = ranking.ByMerit?.Rows;
        var results = CalculateResultsScores(byMeritRows);

        var rankingSummary = new RankingSummary
        {
            HowManyCanEnroll = byMeritRows?.Count(x => x.CanEnroll),
            HowManyStudents = byMeritRows?.Count,
            ResultsSummarized = results,
            CourseSummarized = ranking.ByCourse?.Select(x => x.GetStats()).ToList()
        };

        return rankingSummary;
    }

    private static Dictionary<int, int>? CalculateResultsScores(IReadOnlyCollection<StudentResult>? byMeritRows)
    {
        if (byMeritRows == null) return null;

        var results = new Dictionary<int, int>();
        var enumerable = byMeritRows.Select(variable => (int)Math.Round(variable.Result));
        foreach (var score in enumerable)
        {
            results.TryAdd(score, 0);
            results[score] += 1;
        }

        return results;
    }
}