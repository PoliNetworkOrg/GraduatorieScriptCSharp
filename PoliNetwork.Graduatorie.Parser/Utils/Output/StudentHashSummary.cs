using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Json;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StudentHashSummary
{
    public string? Id;
    public List<SingleCourseJson> SingleCourseJsons = new();
    public List<RankingSummaryStudent> RankingSummaries = new();
    public bool? Merit;
    public void Merge(StudentResult student, Ranking ranking, CourseTable? courseTable)
    {
        if (string.IsNullOrEmpty(Id))
            this.Id = student.Id;

        if (courseTable == null)
        {
            Merit = true;
        }
        else
        {
            var s = ranking.ToSingleCourseJson().FirstOrDefault(x => x.Is(courseTable));
            if (s != null) this.SingleCourseJsons.Add(s);
        }

        var r = ranking.GetRankingSummaryStudent();
        RankingSummaries.Add(r);
    }
}