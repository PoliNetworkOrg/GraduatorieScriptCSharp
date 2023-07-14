﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StudentHashSummary
{
    public string? Id;
    public List<RankingSummaryStudent> SingleCourseJsons = new();
    public List<RankingSummaryStudent> RankingSummaries = new();
    public bool? InMeritTable;
    public void Merge(StudentResult student, Ranking ranking, CourseTable? courseTable)
    {
        if (string.IsNullOrEmpty(Id))
            this.Id = student.Id;

        if (courseTable == null)
        {
            InMeritTable = true;
        }
        else
        {
            var s = courseTable.GetRankingSummaryStudent(ranking);
            bool present1 = this.SingleCourseJsons.Any(x => x.Equals(s));
            if (!present1)
                this.SingleCourseJsons.Add(s);
        }

        var r = ranking.GetRankingSummaryStudent();
        bool present2 = this.RankingSummaries.Any(x => x.Equals(r));
        if (!present2)
            RankingSummaries.Add(r);
    }
}