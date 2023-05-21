﻿using Newtonsoft.Json;

namespace GraduatorieScript.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class Ranking
{
    private Dictionary<string, List<StudentResult>>? byDegree;
    private List<StudentResult>? byId;
    private List<StudentResult>? byMerit;
    private string? extra;
    private DateTime lastUpdate;
    private string? phase;
    private string? school;
    private string? url;
    private string? year;

    public bool IsSimilarTo(Ranking ranking)
    {
        return year == ranking.year &&
               school == ranking.school &&
               phase == ranking.phase &&
               extra == ranking.extra &&
               url == ranking.url;
    }
}