using Newtonsoft.Json;
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
            var present1 = SingleCourseJsons.Any(x => x.Equals(s));
            if (!present1)
                SingleCourseJsons.Add(s);
        }

        var r = ranking.GetRankingSummaryStudent();
        var present2 = RankingSummaries.Any(x => x.Equals(r));
        if (!present2)
            RankingSummaries.Add(r);
    }
}