#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Parser.Utils;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MeritTable
{
    public List<string>? Headers;
    public string? Path;
    public List<StudentResult>? Rows;
    public int? Year;

    public int GetHashWithoutLastUpdate()
    {
        var r = new List<int?> { "MeritTable".GetHashCode() };
        if (Headers != null)
            r.Add(Headers.Aggregate("HeadersFull".GetHashCode(),
                (current, variable) => current ^ variable.GetHashCode()));
        else
            r.Add("HeadersEmpty".GetHashCode());

        if (Rows != null)
            r.Add(Rows.Aggregate("RowsFull".GetHashCode(), (current, variable) =>
            {
                var hashWithoutLastUpdate = variable.GetHashWithoutLastUpdate();
                return current ^ hashWithoutLastUpdate;
            }));
        else
            r.Add("RowsEmpty".GetHashCode());

        r.Add(Year?.GetHashCode() ?? "Year".GetHashCode());
        r.Add(Path?.GetHashCode() ?? "Path".GetHashCode());
        return Hashing.GetHashFromListHash(r);
    }
}