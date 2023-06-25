namespace GraduatorieScript.Objects;

public class MeritTableRow
{
    public bool canEnroll;
    public string? canEnrollInto;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public int position;
    public decimal result;
}

public class CourseTableRow
{
    public DateOnly birthDate;
    public bool canEnroll;
    public int? englishCorrectAnswers;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public Dictionary<string, decimal>? sectionsResults;
    public int position;
    public decimal result;
}
