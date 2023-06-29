﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraduatorieScript.Objects.Json;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class StatsSchool
{
    public List<StatsSingleCourseJson> List = new();
    public int? NumStudents;

    public int GetHashWithoutLastUpdate()
    {
        var i = NumStudents ?? 0;
        foreach (var VARIABLE in List) i ^= VARIABLE.GetHashWithoutLastUpdate();

        return i;
    }
}