using GraduatorieScript.Objects;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

namespace PoliNetwork.Graduatorie.Parser.Utils.Transformer.ParserNS;

public static class Converter
{
    public static StudentResult FromMeritTableToStudentResult(MeritTableRow row)
    {
        var rowCanEnroll = row.CanEnroll ?? false;
        return new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = null,
            CanEnroll = rowCanEnroll,
            CanEnrollInto = rowCanEnroll ? row.CanEnrollInto : null,
            PositionAbsolute = row.Position,
            PositionCourse = null,
            SectionsResults = null,
            EnglishCorrectAnswers = null
        };
    }

    public static StudentResult FromCourseTableRowToStudentResult(CourseTableRow row, Table<CourseTableRow> course)
    {
        var rowCanEnroll = row.CanEnroll ?? false;
        return new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = row.BirthDate,
            CanEnroll = rowCanEnroll,
            CanEnrollInto = rowCanEnroll ? course.CourseTitle : null,
            PositionAbsolute = null,
            PositionCourse = row.Position,
            SectionsResults = row.SectionsResults,
            EnglishCorrectAnswers = row.EnglishCorrectAnswers
        };
    }
}