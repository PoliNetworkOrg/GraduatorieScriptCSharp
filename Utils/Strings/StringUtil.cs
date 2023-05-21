using GraduatorieScript.Objects;

namespace GraduatorieScript.Utils.Strings;

public static class StringUtil
{
    public static HashSetExtended<string> Merge(IEnumerable<HashSetExtended<string>> list)
    {
        //calculate strings to merge
        var toAdd = list.SelectMany(v1 => v1.Where(v2 => !string.IsNullOrEmpty(v2)));

        //merge into HashSetExtended
        var r = new HashSetExtended<string>();
        r.AddRange(toAdd);
        return r;
    }
}