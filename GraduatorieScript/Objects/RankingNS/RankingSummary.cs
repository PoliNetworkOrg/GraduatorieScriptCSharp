﻿using GraduatorieScript.Objects.Tables.Course;
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
            i = CourseSummarized.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
        if (ResultsSummarized != null)
            i = ResultsSummarized.Aggregate(i, (current, variable) => current ^ variable.Key ^ variable.Value);
        return i;
    }

    public static RankingSummary From(Ranking ranking)
    {
        var byMeritRows = ranking.ByMerit?.Rows;
        var results = CalculateResultsScores(byMeritRows);

        var keyValuePairs = results?.OrderBy(x => x.Key)
            .ToDictionary(obj => obj.Key, obj => obj.Value);

        var courseTableStatsList = ranking.ByCourse?.Select(x => x.GetStats())
            .OrderBy(x => x.Title).ThenBy(x => x.Location).ToList();

        var howManyCanEnroll = byMeritRows?.Count(x => x.CanEnroll);

        return new RankingSummary
        {
            HowManyCanEnroll = howManyCanEnroll,
            HowManyStudents = byMeritRows?.Count,
            ResultsSummarized = keyValuePairs,
            CourseSummarized = courseTableStatsList
        };
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