#region

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace PoliNetwork.Graduatorie.Common.Utils.HashNS;

public static partial class HashMatricola
{
    private const string SaltGlobal = "saltPoliNetwork";
    private const int MaxCharHash = 20;

    private static string? CleanInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        if (input.Contains(' ')) input = input.Split(" ").First(x => !string.IsNullOrEmpty(x));

        var s = input.Trim().ToUpper();
        if (string.IsNullOrEmpty(s))
            return null;

        return NotAlphaNumericRegex().Replace(s, "");
    }

    public static string? HashMatricolaMethod(string? input)
    {
        input = CleanInput(input);

        if (string.IsNullOrEmpty(input))
            return null;

        var stringInputWithSalt = input + SaltGlobal;
        var hexHash = GetSha256(stringInputWithSalt);
        var hashMatricolaMethod = hexHash[..MaxCharHash];
        var matricolaMethod = hashMatricolaMethod.ToLower();
        return matricolaMethod;
    }

    private static string GetSha256(string stringInputWithSalt)
    {
        var bytes = Encoding.UTF8.GetBytes(stringInputWithSalt);
        var hashBytes = SHA256.HashData(bytes);
        var hexHash = ByteArrayToHexString(hashBytes);
        return hexHash;
    }

    private static string ByteArrayToHexString(IReadOnlyCollection<byte> bytes)
    {
        var sb = new StringBuilder(bytes.Count * 2);
        foreach (var b in bytes) sb.Append(b.ToString("X2"));
        return sb.ToString();
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NotAlphaNumericRegex();
}