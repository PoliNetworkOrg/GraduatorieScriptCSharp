#region

using System.Globalization;
using Newtonsoft.Json;

#endregion

namespace PoliNetwork.Graduatorie.Common.Data;

public static class Culture
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Culture = CultureInfo.InvariantCulture,
        Formatting = Formatting.Indented
    };

    public static readonly NumberFormatInfo NumberFormatInfo = new() { NumberDecimalSeparator = "." };
}