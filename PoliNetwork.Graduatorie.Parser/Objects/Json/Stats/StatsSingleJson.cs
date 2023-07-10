﻿using GraduatorieScript.Objects.RankingNS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json.Stats;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSingleCourseJson
{
    public SingleCourseJson? SingleCourseJson;
    public RankingSummary? Stats;

    public static List<StatsSingleCourseJson> From(Ranking ranking)
    {
        var singleCourseJsons = ranking.ToSingleCourseJson();
        return singleCourseJsons.Select(variable => new StatsSingleCourseJson
            { SingleCourseJson = variable, Stats = ranking.RankingSummary }).ToList();
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = "StatsSingleCourseJson".GetHashCode();
        i ^= SingleCourseJson?.GetHashWithoutLastUpdate() ?? "SingleCourseJson".GetHashCode();
        i ^= Stats?.GetHashWithoutLastUpdate() ?? "Stats".GetHashCode();

        return i;
    }
}