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
        ;

    }

    private static void AddToDict(IDictionary<string, StudentHashSummary> dictionary, Ranking ranking, StudentResult student, CourseTable? courseTable)
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