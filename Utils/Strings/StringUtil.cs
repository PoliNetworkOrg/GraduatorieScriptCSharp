using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Strings;

public static class StringUtil
{
    public static HashSetExtended<string> Merge(IEnumerable<HashSetExtended<string>> list)
    {
        var r = new HashSetExtended<string>();
        var toAdd = list.SelectMany(v1 => v1.Where(v2 => !string.IsNullOrEmpty(v2)));
        foreach (var v2 in toAdd) r.Add(v2);

        return r;
    }
}