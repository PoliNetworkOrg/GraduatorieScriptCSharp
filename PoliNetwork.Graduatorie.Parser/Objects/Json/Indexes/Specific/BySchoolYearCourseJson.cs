#region

using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PoliNetwork.Graduatorie.Common.Enums;
using PoliNetwork.Graduatorie.Parser.Objects.RankingNS;

#endregion

namespace PoliNetwork.Graduatorie.Parser.Objects.Json.Indexes.Specific;

using SchoolsDict =
    SortedDictionary<SchoolEnum,
        SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>>;
using YearsDict = SortedDictionary<int, SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>>;
using CoursesDict = SortedDictionary<string, SortedDictionary<string, List<SingleCourseJson>>>;
using CourseDict = SortedDictionary<string, List<SingleCourseJson>>;

[Serializable]
[JsonObject(MemberSerialization.Fields, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class BySchoolYearCourseJson : IndexJsonBase
{
    internal const string CustomPath = "bySchoolYearCourse.json";
    public List<SingleCourseJson> All = new(); // decide whether to include it in the json serialization

    //keys: school, year, course, location
    public SchoolsDict Schools = new();

    public static BySchoolYearCourseJson From(RankingsSet set)
    {
        var mainJson = new BySchoolYearCourseJson { LastUpdate = set.LastUpdate };

        var list = set.Rankings
            .SelectMany(r => r.ToSingleCourseJson())
            .DistinctBy(r => new { r.Id, r.Location })
            .ToList();

        list.Sort();
        mainJson.All = list;

        // group rankings by school
        var bySchool = set.Rankings.GroupBy(r => r.School);

        foreach (var schoolGroup in bySchool)
        {
            var school = schoolGroup.Key;

            var byYears = schoolGroup.GroupBy(r => r.Year);
            var yearsDict = GetYearsDict(byYears);

            mainJson.Schools.Add(school, yearsDict);
        }


        return mainJson;
    }

    private static YearsDict GetYearsDict(IEnumerable<IGrouping<int, Ranking>> byYears)
    {
        var yearsDict = new YearsDict();

        foreach (var yearGroup in byYears)
        {
            var coursesDict = GetCoursesDict(yearGroup);
            yearsDict.Add(yearGroup.Key, coursesDict);
        }

        return yearsDict;
    }

    private static CoursesDict GetCoursesDict(IEnumerable<Ranking> yearGroup)
    {
        var coursesDict = new CoursesDict();

        foreach (var ranking in yearGroup)
        {
            var byTitle =
                ranking.ByCourse.Where(c => c.Title != null).GroupBy(c => c.Title!); // e.g. INGEGNERIA AEROSPAZIALE

            foreach (var courseGroup in byTitle)
            {
                var alreadyExisted = coursesDict.ContainsKey(courseGroup.Key);
                var courseDict = alreadyExisted
                    ? coursesDict[courseGroup.Key]
                    : new CourseDict();

                foreach (var courseTable in courseGroup)
                {
                    var location = courseTable.GetFixedLocation();
                    if (!courseDict.ContainsKey(location))
                    {
                        // first time this location is encountered,
                        // so we instantiate the list for this location
                        var newLocationList = new List<SingleCourseJson>();
                        courseDict.Add(location, newLocationList);
                    }

                    var locationList = courseDict.GetValueOrDefault(location);
                    if (locationList == null)
                        throw new UnreachableException(); // this should never happen at this point

                    var singleCourseJson = SingleCourseJson.From(ranking, courseTable);

                    if (locationList.Any(
                            x => x.Id == singleCourseJson.Id && x.Location == singleCourseJson.Location))
                        continue;

                    locationList.Add(singleCourseJson);
                    locationList.Sort();
                }

                if (!alreadyExisted) coursesDict.Add(courseGroup.Key, courseDict);
            }
        }

        return coursesDict;
    }
}