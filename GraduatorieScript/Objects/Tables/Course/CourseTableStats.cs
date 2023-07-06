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
    public decimal? MinScoreToEnroll;
    public string? Title;

    public int GetHashWithoutLastUpdate()
    {
        var i = AverageBirthYear?.GetHashCode() ?? "AverageBirthYear".GetHashCode();
        i ^= AverageEnglishCorrectAnswers?.GetHashCode() ?? "AverageEnglishCorrectAnswers".GetHashCode();
        i ^= AverageOfWhoPassed?.GetHashCode() ?? "AverageOfWhoPassed".GetHashCode();
        i ^= AverageScoresOfAllStudents?.GetHashCode() ?? "AverageScoresOfAllStudents".GetHashCode();
        i ^= Location?.GetHashCode() ?? "Location".GetHashCode();
        i ^= Title?.GetHashCode() ?? "Title".GetHashCode();
        i ^= MinScoreToEnroll?.GetHashCode() ?? "MinScoreToEnroll".GetHashCode();

        if (HowManyOfa == null)
        {
            i ^= "HowManyOfa".GetHashCode();
        }
        else
            foreach (var variable in HowManyOfa)
            {
                i ^= variable.Key.GetHashCode();
                i ^= variable.Value.GetHashCode();
            }


        if (AveragePartialScores == null)
        {
            i ^= "AveragePartialScores".GetHashCode();
        }
        else
        {
            foreach (var variable in AveragePartialScores)
            {
                i ^= variable.Key.GetHashCode();
                i ^= variable.Value.GetHashCode();
            }
        }

        //return result
        return i;
    }

    private static double? GetYearBorn(StudentResult x)
    {
        var birthDateMonth = x.BirthDate?.Month / 12.0;
        var birthDateYear = x.BirthDate?.Year;
        var dateMonth = birthDateYear + birthDateMonth;
        return dateMonth;
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

        var studentsWhoCanEnroll = courseTableRows.Where(x => x.CanEnroll ?? false).ToList();
        var minValueToEnroll =
            studentsWhoCanEnroll.Count > 0 ? studentsWhoCanEnroll.Min(x => x.Result) : (decimal?)null;
        var resultsOfStudentsWhoCanEnroll = studentsWhoCanEnroll.Select(x => x.Result);
        var resultsOfAllStudents = courseTableRows.Select(x => x.Result);
        var yearBornStudents = courseTableRows.Select(GetYearBorn);

        //fill stats field
        stats.AverageScoresOfAllStudents = AverageList(resultsOfAllStudents);
        stats.AverageOfWhoPassed = AverageList(resultsOfStudentsWhoCanEnroll);
        stats.AverageBirthYear = AverageList(yearBornStudents);
        stats.AverageEnglishCorrectAnswers = AverageList(courseTableRows.Select(x => x.EnglishCorrectAnswers).ToList());
        stats.AveragePartialScores = AveragePartialScoresCalculate(courseTableRows);
        stats.HowManyOfa = HowManyOfaCalculate(courseTableRows);
        stats.MinScoreToEnroll = MathRound(minValueToEnroll);
        return stats;
    }

    private static double? AverageList(IEnumerable<double?>? yearBornStudents)
    {
        if (yearBornStudents == null)
            return null;
        
        var bornStudents = yearBornStudents.ToList();
        return !bornStudents.Any() ? null : MathRound(bornStudents.Average());
    }

    private static decimal? AverageList(IEnumerable<decimal?>? testResults)
    {
        if (testResults == null)
            return null;
        
        var enumerable = testResults.ToList();
        return !enumerable.Any() ? null : MathRound(enumerable.Average());
    }

    private static double? AverageList(IReadOnlyCollection<int?>? testResults)
    {
        if (testResults == null)
            return null;
        
        return testResults.Count == 0 ? null : MathRound(testResults.Average());
    }

    private static decimal? AverageList(IReadOnlyCollection<decimal>? testResults)
    {
        if (testResults == null)
            return null;
        
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