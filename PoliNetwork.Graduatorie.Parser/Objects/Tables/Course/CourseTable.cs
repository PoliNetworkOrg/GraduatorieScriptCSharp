﻿#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;
using PoliNetwork.Graduatorie.Parser.Utils.Output;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTable : MeritTable
{
    public string? Location;
    public List<string>? Sections;
    public string? Title;

    public CourseTableStats GetStats()
    {
        return CourseTableStats.From(this);
    }

    public RankingSummaryStudent GetRankingSummaryStudent(Ranking ranking)
    {
        return new RankingSummaryStudent(Title, ranking.RankingOrder?.Phase, ranking.School,
            ranking.Url, ranking.Year);
    }
}