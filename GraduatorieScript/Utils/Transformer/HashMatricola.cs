using System.Security.Cryptography;
using System.Text;

namespace GraduatorieScript.Utils.Transformer;

public static class HashMatricola
{
    private const string SaltGlobal = "saltPoliNetwork";
    private const int MaxCharHash = 20;
    
    public static string? HashMatricolaMethod(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        var stringInputWithSalt = input + SaltGlobal;
        var hexHash = GetSha256(stringInputWithSalt);
        var hashMatricolaMethod = hexHash[..MaxCharHash];
        return hashMatricolaMethod;
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
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        return sb.ToString();
    }
}