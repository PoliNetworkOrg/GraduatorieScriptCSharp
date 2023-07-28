#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingOrder
{
    public bool? Anticipata;
    public bool? ExtraEu;

    public string? Phase;

    //esempio:
    //seconda graduatoria di seconda fase: {primary:2,secondary:2}
    //prima graduatoria di seconda fase:{primary:2, secondary:1}
    public int? Primary;
    public int? Secondary;

    public RankingOrder()
    {
    }

    public RankingOrder(string phase)
    {
        Phase = phase;
        FixValues();
    }

    private void FixValues()
    {
        var s = Phase?.ToUpper().Trim() ?? "";
        if (string.IsNullOrEmpty(s))
            return;

        ExtraEu = s.Contains("EXTRA");
        var strings = s.Split(" ");
        Primary = GetCount(strings, "FASE");
        Secondary = GetCount(strings, "GRADUATORIA");
        Anticipata = s.Contains("ANTICIPATA");
    }

    private static int? GetCount(IReadOnlyList<string> s, string key)
    {
        for (var i = 0; i < s.Count; i++)
        {
            var item = s[i];
            if (item != key) continue;
            if (i - 1 < 0)
                continue;

            var item2 = s[i - 1];
            return item2 switch
            {
                "PRIMA" => 1,
                "SECONDA" => 2,
                "TERZA" => 3,
                "QUARTA" => 4,
                "QUINTA" => 5,
                _ => null
            };
        }

        return null;
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = "RankingOrder".GetHashCode();
        i ^= Phase?.GetHashCode() ?? "Phase".GetHashCode();
        i ^= Primary?.GetHashCode() ?? "Primary".GetHashCode();
        i ^= Secondary?.GetHashCode() ?? "Secondary".GetHashCode();
        i ^= ExtraEu?.GetHashCode() ?? "ExtraEu".GetHashCode();

        return i;
    }

    public void Merge(RankingOrder? rankingRankingOrder)
    {
        throw new NotImplementedException();
    }
}