#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StudentHashSummary
{
    public string? Id;
    public bool? InMeritTable;
    public List<RankingSummaryStudent> RankingSummaries = new();
    public List<RankingSummaryStudent> SingleCourseJsons = new();

    public void Merge(StudentResult student, Ranking ranking, CourseTable? courseTable)
    {
        if (string.IsNullOrEmpty(Id))
            Id = student.Id;

        if (courseTable == null)
        {
            InMeritTable = true;
        }
        else
        {
            var s = courseTable.GetRankingSummaryStudent(ranking);
            var alreadyPresentJson = SingleCourseJsons.Any(x => x.Equals(s));
            if (!alreadyPresentJson) SingleCourseJsons.Add(s);
        }

        var r = ranking.GetRankingSummaryStudent();
        var alreadyPresentSummary = RankingSummaries.Any(x => x.Equals(r));
        if (!alreadyPresentSummary) RankingSummaries.Add(r);
    }

    public void Sort()
    {
        RankingSummaries.Sort();
        SingleCourseJsons.Sort();
    }
}