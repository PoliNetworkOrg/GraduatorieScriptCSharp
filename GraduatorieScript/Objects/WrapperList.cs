namespace GraduatorieScript.Objects;

public class WrapperList<T> 
{
    private List<T?>? items;
    public void Add(T? value)
    {
        this.items ??= new List<T?>();
        this.items?.Add(value);
    }

    public List<T?>? Distinct()
    {
        var list = this.items;
        return list?.Distinct().ToList();
    }

 
}