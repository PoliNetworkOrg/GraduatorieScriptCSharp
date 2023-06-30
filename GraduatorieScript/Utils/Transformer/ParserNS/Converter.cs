using GraduatorieScript.Objects;
using GraduatorieScript.Objects.Tables;

namespace GraduatorieScript.Utils.Transformer.ParserNS;

public class Converter
{
    public static StudentResult FromMeritTableToStudentResult(MeritTableRow row)
    {
        return new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = null,
            CanEnroll = row.CanEnroll,
            CanEnrollInto = row.CanEnroll ? row.CanEnrollInto : null,
            PositionAbsolute = row.Position,
            PositionCourse = null,
            SectionsResults = null,
            EnglishCorrectAnswers = null
        };
    }

    public static StudentResult FromCourseTableRowToStudentResult(CourseTableRow row, Table<CourseTableRow> course)
    {
        return new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = row.BirthDate,
            CanEnroll = row.CanEnroll,
            CanEnrollInto = row.CanEnroll ? course.CourseTitle : null,
            PositionAbsolute = null,
            PositionCourse = row.Position,
            SectionsResults = row.SectionsResults,
            EnglishCorrectAnswers = row.EnglishCorrectAnswers
        };
    }
}