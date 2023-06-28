using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
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