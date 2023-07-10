using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ArgsConfig
{
    public string? DataFolder;
    public bool? ForceReparsing;
}