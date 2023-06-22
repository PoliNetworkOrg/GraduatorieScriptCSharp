namespace GraduatorieScript.Objects;

public class MeritTableRow
{
    public DateOnly birthDate;
    public bool canEnroll;
    public string? canEnrollInto;
    public string? id;
    public Dictionary<string, bool>? ofa; // maybe change it
    public int position;
    public decimal result;
}

public class CourseTableRow
{
    private DateOnly birthDate;
    private bool canEnroll;
    private int englishCorrectAnswers;
    private string? id;
    private Dictionary<string, bool>? ofa; // maybe change it
    private Dictionary<string, decimal>? partialResults;
    private int position;
    private decimal result;
}