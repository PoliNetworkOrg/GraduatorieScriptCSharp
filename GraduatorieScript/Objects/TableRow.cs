namespace GraduatorieScript.Objects;

public class MeritTableRow
{
    public string? id;
    public decimal result;
    public Dictionary<string, bool>? ofa; // maybe change it
    public int position;
    public DateOnly birthDate;
    public bool canEnroll;
    public string? canEnrollInto;
}

public class CourseTableRow
{
    private string? id;
    private int position;
    private DateOnly birthDate;
    private bool canEnroll;
    private decimal result;
    private int englishCorrectAnswers;
    private Dictionary<string, decimal>? partialResults;
    private Dictionary<string, bool>? ofa; // maybe change it
}
