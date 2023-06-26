using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTableStats
{
    public string? Location;
    public string? Title;
    public decimal? AverageScoresOfAllStudents;
    public decimal? AverageOfWhoPassed;
    public double? AverageBirthYear;
    public double? AverageEnglishCorrectAnswers;
    public Dictionary<string, decimal?>? AveragePartialScores;

    public static CourseTableStats From(CourseTable courseTable)
    {
        var courseTableRows = courseTable.Rows;
        return new CourseTableStats()
        {
            Location = courseTable.Location,
            Title = courseTable.Title,
            AverageScoresOfAllStudents = courseTableRows?.Select(x => x.result).Average(),
            AverageOfWhoPassed = courseTableRows?.Where(x => x.canEnroll).Select(x => x.result).Average(),
            AverageBirthYear = courseTableRows?.Select(x => x.birthDate?.Year).Average(),
            AverageEnglishCorrectAnswers = courseTableRows?.Select(x => x.englishCorrectAnswers).Average(),
            AveragePartialScores = AveragePartialScoresCalculate(courseTableRows)
        };
    }

    private static Dictionary<string, decimal?> AveragePartialScoresCalculate(List<StudentResult>? courseTableRows)
    {
        var scores = new Dictionary<string, decimal?>();

        var keys = courseTableRows?.Select(x => x.sectionsResults?.Keys).ToList();
        var keysDistinct = Distinct(keys);

        foreach (var key in keysDistinct)
        {
            scores[key] = AveragePartialScoresOfASingleKey(courseTableRows, key);
        }

        return scores;
    }

    private static decimal? AveragePartialScoresOfASingleKey(List<StudentResult>? courseTableRows, string key)
    {
        return courseTableRows?.Select(x => x.sectionsResults?[key]).Average();
    }

    private static HashSet<string> Distinct(List<Dictionary<string, decimal>.KeyCollection?>? keys)
    {
        var hashSet = new HashSet<string>();
        if (keys == null) return hashSet;
        foreach (var v1 in keys)
        {
            if (v1 == null) continue;
            foreach (var v2 in v1)
            {
                hashSet.Add(v2);
            }
        }

        return hashSet;
    }
}