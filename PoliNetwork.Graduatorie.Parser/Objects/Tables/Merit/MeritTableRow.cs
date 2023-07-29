#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MeritTableRow
{
    public bool? CanEnroll;
    public string? CanEnrollInto;
    public string? Id;
    public SortedDictionary<string, bool>? Ofa; // maybe change it
    public int? Position;
    public decimal? Result;
}