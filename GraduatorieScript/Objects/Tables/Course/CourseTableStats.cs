using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTableStats
{
    public double? AverageBirthYear;
    public double? AverageEnglishCorrectAnswers;
    public Dictionary<string, int>? AverageOfa;
    public decimal? AverageOfWhoPassed;
    public Dictionary<string, decimal>? AveragePartialScores;
    public decimal? AverageScoresOfAllStudents;
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

        if (AverageOfa != null)
            foreach (var variable in AverageOfa)
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

        var testResults = courseTableRows.Select(x => x.Result).ToList();
        stats.AverageScoresOfAllStudents = testResults.Count > 0 ? testResults.Average() : null;

        var passedTestResults = courseTableRows
            .Where(x => x.CanEnroll)
            .Select(x => x.Result)
            .ToList();
        stats.AverageOfWhoPassed = passedTestResults.Count > 0 ? passedTestResults.Average() : null;

        var birthYears = courseTableRows.Select(x => x.BirthDate?.Year).ToList();
        stats.AverageBirthYear = birthYears.Count > 0 ? birthYears.Average() : null;

        var engCorrAnswers = courseTableRows.Select(x => x.EnglishCorrectAnswers).ToList();
        stats.AverageEnglishCorrectAnswers =
            engCorrAnswers.Count > 0 ? engCorrAnswers.Average() : null;

        stats.AveragePartialScores =
            count > 0 ? AveragePartialScoresCalculate(courseTableRows) : null;
        stats.AverageOfa = count > 0 ? AverageOfaCalculate(courseTableRows) : null;
        return stats;
    }

    private static Dictionary<string, int> AverageOfaCalculate(
        IReadOnlyCollection<StudentResult> courseTableRows
    )
    {
        var result = new Dictionary<string, int>();
        if (courseTableRows.Count == 0)
            return result;

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

    private static Dictionary<string, decimal> AveragePartialScoresCalculate(
        IReadOnlyCollection<StudentResult> courseTableRows
    )
    {
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