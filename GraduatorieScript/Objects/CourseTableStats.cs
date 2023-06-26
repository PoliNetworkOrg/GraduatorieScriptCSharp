using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CourseTableStats
{
    public string? Location;
    public string? Title;
    public decimal? Average;
    public decimal? AverageOfWhoPassed;

    public static CourseTableStats From(CourseTable courseTable)
    {
        var courseTableRows = courseTable.Rows;
        return new CourseTableStats()
        {
            Location = courseTable.Location,
            Title = courseTable.Title,
            Average = courseTableRows?.Select(x => x.result).Average(),
            AverageOfWhoPassed = courseTableRows?.Where(x => x.canEnroll).Select(x => x.result).Average()
        };
    }
}