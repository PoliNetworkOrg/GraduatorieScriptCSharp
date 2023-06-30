using System.Globalization;
using Newtonsoft.Json;

namespace GraduatorieScript.Data;

public static class Culture
{
    public static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Culture = CultureInfo.InvariantCulture,
        Formatting = Formatting.Indented
    };
}