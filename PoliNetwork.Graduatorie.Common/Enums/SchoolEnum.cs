#region

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace PoliNetwork.Graduatorie.Common.Enums;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum SchoolEnum
{
    Ingegneria = 1,
    Urbanistica = 2,
    Architettura = 3,
    Design = 4,
    Unknown = 0
}