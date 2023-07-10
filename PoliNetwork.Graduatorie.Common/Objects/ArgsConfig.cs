using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Common.Utils.Path;

namespace PoliNetwork.Graduatorie.Common.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ArgsConfig
{
    public string? DataFolder;
    public bool? ForceReparsing;

    public static ArgsConfig GetArgsConfig(IReadOnlyList<string> args)
    {
        var argsConfig = new ArgsConfig
        {
            DataFolder = GetDataFolder(FindArgString(args, "--data")),
            ForceReparsing = FindArgPresent(args, "--reparse")
        };
        return argsConfig;
    }

    private static string GetDataFolder(string? argsFolder)
    {
        // use it if passed or search the default
        var dataFolder = !string.IsNullOrEmpty(argsFolder)
            ? argsFolder
            : PathUtils.FindFolder(Constants.DataFolder);


        if (!string.IsNullOrEmpty(dataFolder)) return dataFolder;

        // if not found, create it
        Console.WriteLine("[WARNING] dataFolder not found, creating it");
        return PathUtils.CreateAndReturnDataFolder(Constants.DataFolder);
    }

    private static bool? FindArgPresent(IEnumerable<string> args, string reparse)
    {
        return args.Any(x => x == reparse);
    }

    private static string? FindArgString(IReadOnlyList<string> args, string data)
    {
        for (var i = 0; i < args.Count; i++)
        {
            var s = args[i];
            if (s != data) continue;
            if (i + 1 < args.Count) return args[i + 1];
        }

        return null;
    }
}