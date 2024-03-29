#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class CourseTableRow
{
    public DateOnly? BirthDate;
    public bool? CanEnroll;
    public int? EnglishCorrectAnswers;
    public string? Id;
    public SortedDictionary<string, bool>? Ofa; // maybe change it
    public int? Position;
    public decimal? Result;
    public SortedDictionary<string, decimal>? SectionsResults;
}