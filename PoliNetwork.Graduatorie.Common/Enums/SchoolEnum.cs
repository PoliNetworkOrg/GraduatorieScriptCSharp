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

public static class SchoolEnumMethods
{
    public static string ToShortName(this SchoolEnum s)
    {
        return s switch
        {
            SchoolEnum.Architettura => "ARC",
            SchoolEnum.Design => "DES",
            SchoolEnum.Ingegneria => "ENG",
            SchoolEnum.Urbanistica => "URB",
            _ => "UNK"
        };
    }
}