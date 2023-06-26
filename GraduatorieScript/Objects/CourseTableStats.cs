using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTableStats
{
    public double? AverageBirthYear;
    public double? AverageEnglishCorrectAnswers;
    public Dictionary<string, int?>? AverageOfa;
    public decimal? AverageOfWhoPassed;
    public Dictionary<string, decimal?>? AveragePartialScores;
    public decimal? AverageScoresOfAllStudents;
    public string? Location;
    public string? Title;

    public static CourseTableStats From(CourseTable courseTable)
    {
        var courseTableRows = courseTable.Rows;
        var averageScoresOfAllStudents =
            courseTableRows?.Count > 0 ? courseTableRows.Select(x => x.Result).Average() : (decimal?)null;
        var averageOfWhoPassed = courseTableRows?.Count > 0
            ? courseTableRows.Where(x => x.CanEnroll).Select(x => x.Result).Average()
            : (decimal?)null;
        var averageBirthYear = courseTableRows?.Count > 0
            ? courseTableRows.Select(x => x.BirthDate?.Year).Average()
            : null;
        var averageEnglishCorrectAnswers = courseTableRows?.Count > 0
            ? courseTableRows.Select(x => x.EnglishCorrectAnswers).Average()
            : null;
        var averagePartialScoresCalculate =
            courseTableRows?.Count > 0 ? AveragePartialScoresCalculate(courseTableRows) : null;
        var averageOfaCalculate = courseTableRows?.Count > 0 ? AverageOfaCalculate(courseTableRows) : null;
        return new CourseTableStats
        {
            Location = courseTable.Location,
            Title = courseTable.Title,
            AverageScoresOfAllStudents =
                averageScoresOfAllStudents,
            AverageOfWhoPassed = averageOfWhoPassed,
            AverageBirthYear = averageBirthYear,
            AverageEnglishCorrectAnswers = averageEnglishCorrectAnswers,
            AveragePartialScores = averagePartialScoresCalculate,
            AverageOfa = averageOfaCalculate
        };
    }

    private static Dictionary<string, int?> AverageOfaCalculate(IReadOnlyCollection<StudentResult>? courseTableRows)
    {
        var result = new Dictionary<string, int?>();
        var keys = courseTableRows?.Select(x => x.SectionsResults?.Keys);
        var distinctKeys = Distinct(keys);
        foreach (var key in distinctKeys)
            result[key] = courseTableRows?.Select(x => x.Ofa).Select(x =>
            {
                var containsKey = x?.ContainsKey(key) ?? false;
                return containsKey ? x?[key] : null;
            }).Count(x => x ?? false);

        return result;
    }

    private static HashSet<string> Distinct(IEnumerable<Dictionary<string, decimal>.KeyCollection?>? keys)
    {
        var result = new HashSet<string>();
        if (keys == null) return result;
        foreach (var v1 in keys)
        {
            if (v1 == null) continue;
            foreach (var v2 in v1) result.Add(v2);
        }

        return result;
    }

    private static Dictionary<string, decimal?> AveragePartialScoresCalculate(
        IReadOnlyCollection<StudentResult>? courseTableRows)
    {
        var scores = new Dictionary<string, decimal?>();

        var keys = courseTableRows?.Select(x => x.SectionsResults?.Keys).ToList();
        var keysDistinct = Distinct(keys);

        foreach (var key in keysDistinct) scores[key] = AveragePartialScoresOfASingleKey(courseTableRows, key);

        return scores;
    }

    private static decimal? AveragePartialScoresOfASingleKey(IEnumerable<StudentResult>? courseTableRows, string key)
    {
        return courseTableRows?.Select(x => x.SectionsResults?[key]).Average();
    }

    private static HashSet<string> Distinct(List<Dictionary<string, decimal>.KeyCollection?>? keys)
    {
        var hashSet = new HashSet<string>();
        if (keys == null) return hashSet;
        foreach (var v1 in keys)
        {
            if (v1 == null) continue;
            foreach (var v2 in v1) hashSet.Add(v2);
        }

        return hashSet;
    }
}