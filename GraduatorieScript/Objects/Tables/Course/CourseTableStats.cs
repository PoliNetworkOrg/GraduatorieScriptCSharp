using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTableStats
{
    private const int Decimals = 5;
    public double? AverageBirthYear;
    public double? AverageEnglishCorrectAnswers;
    public decimal? AverageOfWhoPassed;
    public Dictionary<string, decimal>? AveragePartialScores;
    public decimal? AverageScoresOfAllStudents;
    public Dictionary<string, int>? HowManyOfa;
    public string? Location;
    public string? Title;

    public int GetHashWithoutLastUpdate()
    {
        var i = AverageBirthYear?.GetHashCode() ?? 0;
        i ^= AverageEnglishCorrectAnswers?.GetHashCode() ?? 0;
        i ^= AverageOfWhoPassed?.GetHashCode() ?? 0;
        i ^= AverageScoresOfAllStudents?.GetHashCode() ?? 0;
        i ^= Location?.GetHashCode() ?? 0;
        i ^= Title?.GetHashCode() ?? 0;

        if (HowManyOfa != null)
            foreach (var variable in HowManyOfa)
            {
                i ^= variable.Key.GetHashCode();
                i ^= variable.Value.GetHashCode();
            }

        if (AveragePartialScores != null)
            foreach (var variable in AveragePartialScores)
            {
                i ^= variable.Key.GetHashCode();
                i ^= variable.Value.GetHashCode();
            }

        //return result
        return i;
    }

    public static CourseTableStats From(CourseTable courseTable)
    {
        var stats = new CourseTableStats
        {
            Location = courseTable.Location,
            Title = courseTable.Title
        };

        var courseTableRows = courseTable.Rows;
        var count = courseTableRows?.Count ?? 0;
        if (count == 0 || courseTableRows is null)
            return stats;

        stats.AverageScoresOfAllStudents = AverageList(courseTableRows.Select(x => x.Result).ToList());
        stats.AverageOfWhoPassed = AverageList(courseTableRows
            .Where(x => x.CanEnroll)
            .Select(x => x.Result)
            .ToList());
        stats.AverageBirthYear = AverageList(courseTableRows.Select(x => x.BirthDate?.Year).ToList());
        stats.AverageEnglishCorrectAnswers = AverageList(courseTableRows.Select(x => x.EnglishCorrectAnswers).ToList());
        stats.AveragePartialScores = AveragePartialScoresCalculate(courseTableRows);
        stats.HowManyOfa = HowManyOfaCalculate(courseTableRows);
        return stats;
    }

    private static double? AverageList(IReadOnlyCollection<int?> testResults)
    {
        return testResults.Count == 0 ? null : MathRound(testResults.Average());
    }

    private static decimal? AverageList(IReadOnlyCollection<decimal> testResults)
    {
        return testResults.Count == 0 ? null : MathRound(testResults.Average());
    }

    private static decimal? MathRound(decimal? average)
    {
        return average == null ? null : Math.Round(average.Value, Decimals);
    }

    private static double? MathRound(double? average)
    {
        return average == null ? null : Math.Round(average.Value, Decimals);
    }

    private static Dictionary<string, int>? HowManyOfaCalculate(
        IReadOnlyCollection<StudentResult> courseTableRows
    )
    {
        if (courseTableRows.Count == 0)
            return null;

        var result = new Dictionary<string, int>();

        var keys = courseTableRows.Select(x => x.Ofa?.Keys);
        var distinctKeys = DistinctKeys(keys);
        foreach (var key in distinctKeys)
            result[key] = courseTableRows
                .Select(x => x.Ofa)
                .Where(x => x?.ContainsKey(key) ?? false)
                .Select(x => x?[key])
                .Count(x => x ?? false);

        return result;
    }

    private static Dictionary<string, decimal>? AveragePartialScoresCalculate(
        IReadOnlyCollection<StudentResult> courseTableRows
    )
    {
        if (courseTableRows.Count == 0)
            return null;

        var scores = new Dictionary<string, decimal>();

        var keys = courseTableRows.Select(x => x.SectionsResults?.Keys).ToList();
        var keysDistinct = DistinctKeys(keys);

        foreach (var key in keysDistinct)
        {
            var avg = courseTableRows
                .Select(x => x.SectionsResults)
                .Where(x => x?.ContainsKey(key) ?? false)
                .Select(x => x?[key])
                .Average();
            avg = MathRound(avg);
            if (avg != null) scores[key] = (decimal)avg;
        }

        return scores;
    }


    private static HashSet<string> DistinctKeys<T>(
        IEnumerable<Dictionary<string, T>.KeyCollection?>? keysList
    )
    {
        var result = new HashSet<string>();
        if (keysList == null)
            return result;
        foreach (var keys in keysList)
        {
            if (keys == null)
                continue;
            foreach (var key in keys)
                result.Add(key);
        }

        return result;
    }
}