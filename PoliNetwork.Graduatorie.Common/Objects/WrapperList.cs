#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class WrapperList<T>
{
    public List<T> Items = new();

    public void Add(T value)
    {
        Items.Add(value);
    }

    public List<T> Distinct()
    {
        var list = Items;
        return list.Distinct().ToList();
    }
}