namespace PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

public class RankingOrder
{
    //esempio:
    //seconda graduatoria di seconda fase: {primary:2,secondary:2}
    //prima graduatoria di seconda fase:{primary:2, secondary:1}
    public int? Primary;
    public int? Secondary;
    public string? Phase;
    public bool? ExtraEu;

    public RankingOrder()
    {
        
    }
    
    public RankingOrder(string phase)
    {
        this.Phase = phase;
        FixValues();
    }

    private void FixValues()
    {
        var s = this.Phase?.ToUpper().Trim() ?? "";
        if (string.IsNullOrEmpty(s))
            return;

        this.ExtraEu = s.Contains("EXTRA");
        var strings = s.Split(" ");
        this.Primary = GetCount(strings, "FASE");
        this.Secondary = GetCount(strings, "GRADUATORIA");
    }

    private int? GetCount(string[] s, string key)
    {
        for (int i = 0; i < s.Length; i++)
        {
            var item = s[i];
            if (item == key)
            {
                if (i - 1 >= 0)
                {
                    var item2 = s[i - 1];
                    switch (item2)
                    {
                        case "PRIMA":
                            return 1;
                        case "SECONDA":
                            return 2;
                        case "TERZA":
                            return 3;
                        case "QUARTA":
                            return 4;
                        case "QUINTA":
                            return 5;
                        default:
                            return null;
                    }
                }
            }
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