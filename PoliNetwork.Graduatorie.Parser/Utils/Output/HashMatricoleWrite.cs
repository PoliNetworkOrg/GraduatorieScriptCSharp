using PoliNetwork.Graduatorie.Parser.Objects;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;
using PoliNetwork.Graduatorie.Parser.Objects.Tables.Course;

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

public static class HashMatricoleWrite
{
    public static void Write(RankingsSet? rankingsSet, string outFolder)
    {
        if (rankingsSet == null)
            return;

        var dictionary = GetDictToWrite(rankingsSet);
        WriteToFile(dictionary, outFolder);
    }

    private static Dictionary<string, StudentHashSummary> GetDictToWrite(RankingsSet rankingsSet)
    {
        Dictionary<string, StudentHashSummary> dictionary = new Dictionary<string, StudentHashSummary>();
        foreach (var ranking in rankingsSet.Rankings)
        {
            var byMeritRows = ranking.ByMerit?.Rows;
            if (byMeritRows != null)
                foreach (var student in byMeritRows)
                {
                    if (!string.IsNullOrEmpty(student.Id))
                    {
                        AddToDict(dictionary, ranking, student, null);
                    }
                }

            var rankingByCourse = ranking.ByCourse;
            if (rankingByCourse != null)
            {
                foreach (var courseTable in rankingByCourse)
                {
                    var row = courseTable.Rows;
                    if (row != null)
                    {
                        foreach (var studentResult in row)
                        {
                            if (!string.IsNullOrEmpty(studentResult.Id))
                            {
                                AddToDict(dictionary, ranking, studentResult, courseTable);
                            }
                        }
                    }
                }
            }
        }

        return dictionary;
    }

    private static void WriteToFile(Dictionary<string, StudentHashSummary> dictionary, string outFolder)
    {
        Console.WriteLine($"[INFO] Students with id are {dictionary.Keys.Count}");


        Dictionary<string, Dictionary<string, StudentHashSummary>> dictResult =
            new Dictionary<string, Dictionary<string, StudentHashSummary>>();

        foreach (var variable in dictionary)
        {
            var key = variable.Key[..2];
            if (!dictResult.ContainsKey(key))
                dictResult[key] = new Dictionary<string, StudentHashSummary>();

            if (!dictResult[key].ContainsKey(variable.Key))
                dictResult[key][variable.Key] = variable.Value;
        }

        var hashmatricole = outFolder + "/hashMatricole";
        if (!Directory.Exists(hashmatricole))
        {
            Directory.CreateDirectory(hashmatricole);
        }

        foreach (var variable in dictResult)
        {
            var toWrite = Newtonsoft.Json.JsonConvert.SerializeObject(variable.Value);
            File.WriteAllText(hashmatricole + "/" + variable.Key + ".json", toWrite);
        }
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