namespace GraduatorieScript.Objects;

public class WrapperList<T>
{
    private List<T?>? items;

    public void Add(T? value)
    {
        items ??= new List<T?>();
        items?.Add(value);
    }

    public List<T?>? Distinct()
    {
        var list = items;
        return list?.Distinct().ToList();
    }
}