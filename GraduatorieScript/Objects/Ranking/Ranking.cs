﻿using System.Globalization;
using GraduatorieScript.Enums;
using GraduatorieScript.Objects.Json;
using GraduatorieScript.Objects.Json.Stats;
using GraduatorieScript.Objects.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class Ranking
{
    public List<CourseTable>? ByCourse;
    public MeritTable? ByMerit;
    public string? Extra;
    public DateTime LastUpdate;
    public string? Phase;
    public RankingSummary? RankingSummary;
    public SchoolEnum? School;
    public RankingUrl? Url;
    public int? Year;

    public bool IsSimilarTo(Ranking ranking)
    {
        return Year == ranking.Year &&
               School == ranking.School &&
               Phase == ranking.Phase &&
               Extra == ranking.Extra &&
               Url?.Url == ranking.Url?.Url;
    }


    public void Merge(Ranking ranking)
    {
        LastUpdate = LastUpdate > ranking.LastUpdate ? LastUpdate : ranking.LastUpdate;
        Year ??= ranking.Year;
        Extra ??= ranking.Extra;
        School ??= ranking.School;
        Phase ??= ranking.Phase;
        ByCourse ??= ranking.ByCourse;
        ByMerit ??= ranking.ByMerit;
        Url ??= ranking.Url;
    }

    public string ConvertPhaseToFilename()
    {
        var s = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + "Z";
        var phase1 = Phase ?? s;
        return $"{phase1}.json".Replace(" ", "_");
    }

    public SingleCourseJson ToSingleCourseJson()
    {
        var schoolString = School == null ? null : Enum.GetName(typeof(SchoolEnum), School);
        return new SingleCourseJson
        {
            Link = ConvertPhaseToFilename(),
            Name = Phase,
            BasePath = schoolString + "/" + Year + "/",
            Year = Year,
            School = School
        };
    }

    public StatsSingleCourseJson ToStats()
    {
        return StatsSingleCourseJson.From(this);
    }

    public RankingSummary CreateSummary()
    {
        return RankingSummary.From(this);
    }
}