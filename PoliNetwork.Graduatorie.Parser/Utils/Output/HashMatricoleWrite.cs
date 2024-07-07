#region

using Newtonsoft.Json;
using PoliNetwork.Graduatorie.Common.Data;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Utils.Output;

using IdsDict = SortedDictionary<string, StudentHashSummary>;

public class HashMatricoleWrite
{
    internal const string FolderName = "hashMatricole";
    public IdsDict IdsDict = new();

    public static HashMatricoleWrite From(RankingsSet rankingsSet)
    {
        return new HashMatricoleWrite
        {
            IdsDict = GetIdsDict(rankingsSet)
        };
    }
    

    public void Write(string outFolder)
    {
        Console.WriteLine($"[INFO] Students with id are {IdsDict.Keys.Count}");

        var groupsDict = GetGroupsDict();
        var hashMatricoleFolder = Path.Join(outFolder, FolderName);
        if (!Directory.Exists(hashMatricoleFolder)) Directory.CreateDirectory(hashMatricoleFolder);

        foreach (var (id, idsDict) in groupsDict)
        {
            var idsDictJson = JsonConvert.SerializeObject(idsDict, Culture.JsonSerializerSettings);
            var filename = $"{id}.json";
            var fullPath = Path.Join(hashMatricoleFolder, filename);
            File.WriteAllText(fullPath, idsDictJson);
        }
    }

    private static IdsDict GetIdsDict(RankingsSet rankingsSet)
    {
        var dictionary = new IdsDict();
        foreach (var ranking in rankingsSet.Rankings)
        {
            var byMeritRows = ranking.ByMerit?.Rows;
            if (byMeritRows != null)
                foreach (var student in byMeritRows.Where(student => !string.IsNullOrEmpty(student.Id)))
                {
                    var id = student.Id!;
                    if (!dictionary.ContainsKey(id)) dictionary.Add(id, new StudentHashSummary());
                    dictionary[id].Merge(student, ranking, null);
                }

            var rankingByCourse = ranking.ByCourse;
            if (rankingByCourse == null) continue;
            foreach (var courseTable in rankingByCourse.Where(c => c.Rows != null))
            {
                var row = courseTable.Rows!;
                foreach (var student in row.Where(studentResult => !string.IsNullOrEmpty(studentResult.Id)))
                {
                    var id = student.Id!;
                    
                    if (!dictionary.ContainsKey(id)) dictionary.Add(id, new StudentHashSummary());
                    dictionary[id].Merge(student, ranking, courseTable);
                }
            }
        }

        foreach (var item in dictionary.Values)
        {
            item.Sort();
        }

        return dictionary;
    }

    private SortedDictionary<string, IdsDict> GetGroupsDict()
    {
        var groupsDict = new SortedDictionary<string, IdsDict>();
        var groups = IdsDict.GroupBy(pair => pair.Key[..2]);

        foreach (var group in groups)
        {
            var groupId = group.Key;
            var groupVal = group.ToList();

            var groupIdsDict = new IdsDict();
            foreach (var (id, studentHashSummary) in groupVal)
            {
                groupIdsDict.Add(id, studentHashSummary);
            }
            
            groupsDict.Add(groupId, groupIdsDict);
        }

        return groupsDict;
    }
}