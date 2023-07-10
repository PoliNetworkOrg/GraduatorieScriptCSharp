using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoliNetwork.Graduatorie.Scraper.Utils.Web;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal struct AnchorElement
{
    public string Name;
    public string Url;
}