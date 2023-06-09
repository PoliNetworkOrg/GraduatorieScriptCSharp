﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PoliNetwork.Graduatorie.Parser.Objects.Tables.Merit;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MeritTable
{
    public List<string>? Headers;
    public string? Path;
    public List<StudentResult>? Rows;
    public int? Year;

    public int GetHashWithoutLastUpdate()
    {
        var i = "MeritTable".GetHashCode();
        if (Headers != null)
            i = Headers.Aggregate(i, (current, variable) => current ^ variable.GetHashCode());
        else
            i ^= "Headers".GetHashCode();

        if (Rows != null)
            i = Rows.Aggregate(i, (current, variable) => current ^ variable.GetHashWithoutLastUpdate());
        else
            i ^= "Rows".GetHashCode();

        i ^= Year?.GetHashCode() ?? "Year".GetHashCode();
        i ^= Path?.GetHashCode() ?? "Path".GetHashCode();
        return i;
    }
}