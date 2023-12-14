#region

using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

public static class HashMatricoleWrite
{
    public static void Write(RankingsSet? rankingsSet, string outFolder)
    {
        if (rankingsSet == null)
            return;

        var dictionary = GetDictToWrite(rankingsSet);
        Sort2(dictionary);
        WriteToFile(dictionary, outFolder);
    }
    
    private static void Sort2(SortedDictionary<string, StudentHashSummary> dict)
    {
        var keys = dict.Keys;
        foreach (var key in keys)
        {
            var item = dict[key];
            item.Sort2();
        }
    }

    private static SortedDictionary<string, StudentHashSummary> GetDictToWrite(RankingsSet rankingsSet)
    {
        var dictionary = new SortedDictionary<string, StudentHashSummary>();
        foreach (var ranking in rankingsSet.Rankings)
        {
            var byMeritRows = ranking.ByMerit?.Rows;
            if (byMeritRows != null)
                foreach (var student in byMeritRows.Where(student => !string.IsNullOrEmpty(student.Id)))
                    AddToDict(dictionary, ranking, student, null);

            var rankingByCourse = ranking.ByCourse;
            if (rankingByCourse == null) continue;
            foreach (var courseTable in rankingByCourse)
            {
                var row = courseTable.Rows;
                if (row == null) continue;
                foreach (var studentResult in row.Where(studentResult => !string.IsNullOrEmpty(studentResult.Id)))
                    AddToDict(dictionary, ranking, studentResult, courseTable);
            }
        }

        return dictionary;
    }

    private static void WriteToFile(SortedDictionary<string, StudentHashSummary> dictionary, string outFolder)
    {
        Console.WriteLine($"[INFO] Students with id are {dictionary.Keys.Count}");


        var dictResult =
            new SortedDictionary<string, SortedDictionary<string, StudentHashSummary>>();

        foreach (var variable in dictionary)
        {
            var key = variable.Key[..2];
            if (!dictResult.ContainsKey(key))
                dictResult[key] = new SortedDictionary<string, StudentHashSummary>();

            if (!dictResult[key].ContainsKey(variable.Key))
                dictResult[key][variable.Key] = variable.Value;
        }

        var hashMatricole = outFolder + "/hashMatricole";
        if (!Directory.Exists(hashMatricole)) Directory.CreateDirectory(hashMatricole);

        foreach (var variable in dictResult) WriteSingleHashFile(variable, hashMatricole);
    }

    private static void WriteSingleHashFile(KeyValuePair<string, SortedDictionary<string, StudentHashSummary>> variable,
        string hashMatricole)
    {
        var studentHashSummaries = variable.Value;
        var toWrite = JsonConvert.SerializeObject(studentHashSummaries, Culture.JsonSerializerSettings);
        File.WriteAllText(hashMatricole + "/" + variable.Key + ".json", toWrite);
    }

    private static void AddToDict(IDictionary<string, StudentHashSummary> dictionary, Ranking ranking,
        StudentResult student, CourseTable? courseTable)
    {
        var id = student.Id;
        if (string.IsNullOrEmpty(id))
            return;

        if (dictionary.TryGetValue(id, out var studentPresent))
        {
            studentPresent.Merge(student, ranking, courseTable);
        }
        else
        {
            var studentHashSummary = new StudentHashSummary();
            studentHashSummary.Merge(student, ranking, courseTable);
            dictionary[id] = studentHashSummary;
        }
    }
}