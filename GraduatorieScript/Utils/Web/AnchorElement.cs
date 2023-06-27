using Newtonsoft.Json;

namespace GraduatorieScript.Utils.Web;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
internal struct AnchorElement
{
    public string Name;
    public string Url;
}