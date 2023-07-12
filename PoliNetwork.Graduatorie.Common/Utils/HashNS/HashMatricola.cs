using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PoliNetwork.Graduatorie.Common.Utils.HashNS;

public static class HashMatricola
{
    private const string SaltGlobal = "saltPoliNetwork";
    private const int MaxCharHash = 20;

    private static string RemoveNonAlphanumeric(string input)
    {
        // Remove non-alphanumeric characters using regular expressions
        return Regex.Replace(input, "[^a-zA-Z0-9]", "");
    }
    
    public static string? HashMatricolaMethod(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        input = input.Trim();

        input = RemoveNonAlphanumeric(input);

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
}