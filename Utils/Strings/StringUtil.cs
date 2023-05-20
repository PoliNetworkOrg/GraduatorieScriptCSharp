namespace GraduatorieScript.Utils.Strings;

public static class StringUtil
{
    public static HashSet<string> Merge(IEnumerable<HashSet<string>> list)
    {
        var r = new HashSet<string>();
        foreach (var v2 in list.SelectMany(v1 => v1.Where(v2 => !string.IsNullOrEmpty(v2))))
        {
            r.Add(v2);
        }

        return r;
    }
}
