namespace GraduatorieScript.Objects;

public class WrapperList<T>
{
    private List<T> items = new();

    public void Add(T value)
    {
        items.Add(value);
    }

    public List<T> Distinct()
    {
        var list = items;
        return list.Distinct().ToList();
    }
}
