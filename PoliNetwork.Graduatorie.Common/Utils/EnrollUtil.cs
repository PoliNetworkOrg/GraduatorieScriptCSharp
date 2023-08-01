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
        const string sep = " - ";
        if (!rowCanEnrollInto.Contains(sep))
            return new EnrollType { CanEnroll = true, Course = rowCanEnrollInto, Type = null };

        var s = rowCanEnrollInto.Split(sep);
        var type = s.FirstOrDefault(x => !string.IsNullOrEmpty(x));
        var course = s.FirstOrDefault(x => !string.IsNullOrEmpty(x) && x != type);
        return new EnrollType { CanEnroll = true, Course = course, Type = type };
    }
}