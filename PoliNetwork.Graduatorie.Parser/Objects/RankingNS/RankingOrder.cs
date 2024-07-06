#region

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RankingOrder
{
    public bool IsAnticipata; // used for DES/URB rankings until 2023
    public bool IsEnglish;
    public bool IsExtraEu;
    public string? Phase; // the original string (e.g. "

    //esempio:
    //seconda graduatoria di seconda fase: {primary:2,secondary:2}
    //prima graduatoria di seconda fase:{primary:2, secondary:1}
    public int? Primary;
    public int? Secondary;

    public RankingOrder(string phase, bool isExtraEu = false, bool isEnglish = false)
    {
        Phase = phase;
        ParsePhaseString(phase);

        IsExtraEu = isExtraEu;
        IsEnglish = isEnglish;
    }

    private void ParsePhaseString(string phase)
    {
        var s = phase.ToUpper().Trim();
        if (string.IsNullOrEmpty(s)) return;

        var strings = s.Split(" ");

        IsAnticipata = s.Contains("ANTICIPATA");
        if (IsAnticipata) return;

        Primary = ExtractPhaseNumberByKey(strings, "FASE");
        Secondary = ExtractPhaseNumberByKey(strings, "GRADUATORIA");
    }

    private static int? ExtractPhaseNumberByKey(IReadOnlyList<string> s, string key)
    {
        for (var i = 1; i < s.Count; i++)
        {
            var curr = s[i];
            if (curr != key) continue;

            var prev = s[i - 1];
            return prev switch
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
        if (IsAnticipata) idList.Add("anticipata");
        if (Primary != null) idList.Add($"{Primary}fase");
        if (Secondary != null) idList.Add($"{Secondary}grad");

        var cleanPhase = Phase?.Replace("_", "").Replace("-", "").Replace(" ", "_").ToLower() ?? "";
        var noOrder = IsAnticipata == false && Primary == null && Secondary == null;
        var isSingleExtraEu = noOrder && cleanPhase.Contains("extraue");

        if (noOrder) idList.Add(isSingleExtraEu ? "extraeu" : cleanPhase);

        idList.Add(IsEnglish ? "eng" : "ita");
        if (IsExtraEu && !isSingleExtraEu) idList.Add("extraeu"); // the second condition is to avoid double extraeu

        var id = string.Join("_", idList);
        return id;
    }

    public int GetHashWithoutLastUpdate()
    {
        var i = "RankingOrder".GetHashCode();
        i ^= Phase?.GetHashCode() ?? "Phase".GetHashCode();
        i ^= Primary?.GetHashCode() ?? "Primary".GetHashCode();
        i ^= Secondary?.GetHashCode() ?? "Secondary".GetHashCode();
        i ^= IsExtraEu.GetHashCode();

        return i;
    }

    public void Merge(RankingOrder? rankingRankingOrder)
    {
        Phase ??= rankingRankingOrder?.Phase;
        Primary ??= rankingRankingOrder?.Primary;
        Secondary ??= rankingRankingOrder?.Secondary;
    }
}