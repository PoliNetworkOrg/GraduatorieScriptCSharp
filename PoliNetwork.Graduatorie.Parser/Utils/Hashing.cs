namespace PoliNetwork.Graduatorie.Parser.Utils;

public static class Hashing
{
    public static int GetHashFromListHash(IReadOnlyCollection<int?>? iMerit)
    {
        if (iMerit == null)
            return 0;
        if (iMerit.Count == 0)
            return 0;

        var hashFromListHash = iMerit.Aggregate(0, (current, variable) => current ^ variable ?? 0);
        return hashFromListHash;
    }

    public static int GetHashFromListHash(List<int> iMerit)
    {
        if (iMerit.Count == 0)
            return 0;

        var hashFromListHash = iMerit.Aggregate(0, (current, variable) => current ^ variable);
        return hashFromListHash;
    }
}