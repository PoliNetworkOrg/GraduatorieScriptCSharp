#region

using PoliNetwork.Graduatorie.Common.Utils;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Transformer.ParserNS;

public static class Converter
{
    public static StudentResult FromMeritTableToStudentResult(MeritTableRow row)
    {
        var rowCanEnroll = row.CanEnroll ?? false;
        var rowCanEnrollInto = rowCanEnroll ? row.CanEnrollInto : null;
        return new StudentResult
        {
            Id = row.Id,
            Ofa = row.Ofa,
            Result = row.Result,
            BirthDate = null,
            PositionAbsolute = row.Position,
            PositionCourse = null,
            SectionsResults = null,
            EnglishCorrectAnswers = null,
            EnrollType = EnrollUtil.GetEnrollType(rowCanEnrollInto, rowCanEnroll)
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
            EnrollType = EnrollUtil.GetEnrollType(course.CourseTitle, rowCanEnroll),
            PositionAbsolute = null,
            PositionCourse = row.Position,
            SectionsResults = row.SectionsResults,
            EnglishCorrectAnswers = row.EnglishCorrectAnswers
        };
    }
}