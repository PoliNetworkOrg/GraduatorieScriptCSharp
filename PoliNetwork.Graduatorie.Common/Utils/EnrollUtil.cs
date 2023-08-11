#region

using PoliNetwork.Graduatorie.Common.Objects;

#endregion

namespace PoliNetwork.Graduatorie.Common.Utils;

public class EnrollUtil
{
    public static EnrollType GetEnrollType(string? rowCanEnrollInto, bool rowCanEnroll)
    {
        if (rowCanEnroll == false)
            return new EnrollType { CanEnroll = false, Course = null, Type = null };

        if (string.IsNullOrEmpty(rowCanEnrollInto))
            return new EnrollType { CanEnroll = true, Course = null, Type = null };

        string[] tester = { "assegnato", "prenotato" };
        const string sep = " - ";
        if (!rowCanEnrollInto.Contains(sep) || !tester.Any(t => rowCanEnrollInto.ToLower().Contains(t)))
            return new EnrollType { CanEnroll = true, Course = rowCanEnrollInto, Type = null };

        var s = rowCanEnrollInto.Split(sep).ToList();
        var type = s.FirstOrDefault(x => tester.Any(t => t == x.ToLower()));

        if (type != null) s.Remove(type);

        var course = string.Join(sep, s);
        return new EnrollType { CanEnroll = true, Course = course, Type = type };
    }
}