using Newtonsoft.Json;

namespace GraduatorieScript.Objects;
[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class RankingsSet
{
    private List<Ranking> Rankings;
    private DateTime lastUpdate;

    public static RankingsSet Merge(List<RankingsSet?> list)
    {
        //todo: unire una lista di liste di graduatorie
        throw new NotImplementedException();
    }
}