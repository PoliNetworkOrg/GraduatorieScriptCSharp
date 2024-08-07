﻿#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingSummary
{
    public List<CourseTableStats>? CourseSummarized;
    public int? HowManyCanEnroll;
    public int? HowManyStudents;
    public SortedDictionary<int, int>? ResultsSummarized; //key=score, value=howManyGotThatScore

    public int GetHashWithoutLastUpdate()
    {
        var i = (HowManyStudents ?? "HowManyStudents".GetHashCode()) ^
                (HowManyCanEnroll ?? "HowManyCanEnroll".GetHashCode());
        if (CourseSummarized != null)
            i = CourseSummarized.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
        if (ResultsSummarized != null)
            i = ResultsSummarized.Aggregate(i, (current, variable) => current ^ variable.Key ^ variable.Value);
        return i;
    }

    public static RankingSummary From(Ranking ranking)
    {
        var byMeritRows = ranking.ByMerit.Rows;
        var results = CalculateResultsScores(byMeritRows);

        var keyValuePairs = results?.OrderBy(x => x.Key)
            .ToDictionary(obj => obj.Key, obj => obj.Value);

        var courseTableStatsList = ranking.ByCourse.Select(x => x.GetStats())
            .OrderBy(x => x.Title).ThenBy(x => x.Location).ToList();

        var howManyCanEnroll = byMeritRows?.Count(x => x.EnrollType?.CanEnroll ?? false);


        var groupBy = courseTableStatsList.GroupBy(x =>
        {
            TitleLocation titleLocation;
            titleLocation.Title = x.Title;
            titleLocation.Location = x.Location;
            return titleLocation;
        });
        var distinctBy = groupBy
            .DistinctBy(x =>
            {
                TitleLocation titleLocation;
                titleLocation.Title = x.Key.Title;
                titleLocation.Location = x.Key.Location;
                return titleLocation;
            });
        var tableStatsList = distinctBy.ToList();
        var tableStatsList2 = Get(tableStatsList);
        var resultsSummarized = new SortedDictionary<int, int>(keyValuePairs ?? new Dictionary<int, int>());
        return new RankingSummary
        {
            HowManyCanEnroll = howManyCanEnroll,
            HowManyStudents = byMeritRows?.Count,
            ResultsSummarized = resultsSummarized,
            CourseSummarized = tableStatsList2
        };
    }

    private static List<CourseTableStats> Get(
        IReadOnlyCollection<IGrouping<TitleLocation, CourseTableStats>>? tableStatsList)
    {
        var r = new List<CourseTableStats>();
        if (tableStatsList == null) return r;
        var enumerable = tableStatsList.Select(v1 => v1.ToList());
        foreach (var y in enumerable) r.AddRange(y);

        return r;
    }


    private static SortedDictionary<int, int>? CalculateResultsScores(IReadOnlyCollection<StudentResult>? byMeritRows)
    {
        if (byMeritRows == null) return null;

        var results = new SortedDictionary<int, int>();
        var enumerable = byMeritRows.Select(Round);
        foreach (var score in enumerable)
        {
            if (score == null) continue;
            results.TryAdd(score.Value, 0);
            results[score.Value] += 1;
        }

        return results;
    }

    private static int? Round(StudentResult variable)
    {
        var variableResult = variable.Result;
        if (variableResult == null)
            return null;
        return (int)Math.Round(variableResult.Value);
    }

    private struct TitleLocation
    {
        public string? Title;
        public string? Location;
    }
}