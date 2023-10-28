namespace PoliNetwork.Graduatorie.Parser.Utils;

public static class Hashing
{
    public static int? GetHashFromListHash(IReadOnlyCollection<int?>? iMerit)
    {
        if (iMerit == null)
            return null;
        if (iMerit.Count == 0)
            return null;

        return iMerit.Aggregate(0, (current, variable) => current ^ variable ?? 0);
    }
}