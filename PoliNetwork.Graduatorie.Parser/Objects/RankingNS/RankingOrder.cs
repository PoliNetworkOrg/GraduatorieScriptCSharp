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
    public bool IsEnglish = false;

    public RankingOrder()
    {
    }

    public RankingOrder(string phase, bool isEnglish = false)
    {
        Phase = phase;
        IsEnglish = isEnglish;
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
                "SESTA" => 6,
                "SETTIMA" => 7,
                "OTTAVA" => 8,
                // veramente ci sarà una nona graduatoria?
                _ => null
            };
        }

        return null;
    }

    public string GetId()
    {
        var idList = new List<string>();
        if (Anticipata == true) idList.Add($"anticipata");
        if (Primary != null) idList.Add($"{Primary}fase");
        if (Secondary != null) idList.Add($"{Secondary}grad");
        
        var cleanPhase = Phase?.Replace("_", "").Replace("-", "").Replace(" ", "_").ToLower() ?? "";
        var noOrder = Anticipata == false && Primary == null && Secondary == null; 
        var isSingleExtraEu = noOrder && cleanPhase.Contains("extraue");

        if (noOrder)
        { 
            idList.Add(isSingleExtraEu ? "extraeu" : cleanPhase);
        }
        
        idList.Add(IsEnglish ? "eng" : "ita");
        if (ExtraEu == true && !isSingleExtraEu) idList.Add("extraeu"); // the second condition is to avoid double extraeu
        
        var id = string.Join("_", idList);
        return id;
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
        Anticipata ??= rankingRankingOrder?.Anticipata;
        Phase ??= rankingRankingOrder?.Phase;
        Primary ??= rankingRankingOrder?.Primary;
        Secondary ??= rankingRankingOrder?.Secondary;
        ExtraEu ??= rankingRankingOrder?.ExtraEu;
    }
}
