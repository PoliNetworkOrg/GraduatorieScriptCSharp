using GraduatorieScript.Extensions;

namespace GraduatorieScript.Utils.Strings;

public static class StringUtil
{
    public static HashSet<string> Merge(IEnumerable<HashSet<string>> list)
    {
        //calculate strings to merge
        var toAdd = list.SelectMany(v1 => v1.Where(v2 => !string.IsNullOrEmpty(v2)));

        //merge into HashSet
        var r = new HashSet<string>();
        r.AddRange(toAdd);
        return r;
    }
}
