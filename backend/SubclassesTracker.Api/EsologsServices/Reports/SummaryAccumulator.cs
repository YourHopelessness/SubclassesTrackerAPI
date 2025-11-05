using SubclassesTracker.Models.Responses.Api;
using SubclassesTracker.Models.Responses.Esologs;
using System.Collections.Concurrent;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public sealed class SkillLineAgg
    {
        public int PlayersUsingThisLine;
        public int UniqueSkillsCount;
        public HashSet<SkillApiResponse> Skills = [];
    }

    public sealed class SummaryAccumulator
    {
        private readonly ConcurrentDictionary<string, SkillLineAgg> dd = [];
        private readonly ConcurrentDictionary<string, SkillLineAgg> healer = [];
        private readonly ConcurrentDictionary<string, SkillLineAgg> tank = [];

        private static void AddRange(ConcurrentDictionary<string, SkillLineAgg> dict, IEnumerable<SkillLinesApiResponse> src)
        {
            foreach (var l in src)
            {
                dict.AddOrUpdate(l.LineName,
                    addValueFactory: _ => new SkillLineAgg
                    {
                        PlayersUsingThisLine = l.PlayersUsingThisLine,
                        UniqueSkillsCount = l.UniqueSkillsCount,
                        Skills = [.. l.Skills]
                    },
                    updateValueFactory: (_, agg) =>
                    {
                        agg.PlayersUsingThisLine += l.PlayersUsingThisLine;
                        agg.UniqueSkillsCount = Math.Max(agg.UniqueSkillsCount, l.UniqueSkillsCount);
                        if (l.Skills != null) foreach (var s in l.Skills) agg.Skills.Add(s);
                        return agg;
                    });
            }
        }

        public void AddTrial(SkillLineReportEsologsResponse trial)
        {
            AddRange(dd, trial.DdLinesModels);
            AddRange(healer, trial.HealersLinesModels);
            AddRange(tank, trial.TanksLinesModels);
        }

        private static List<SkillLinesApiResponse> ToLines(IEnumerable<KeyValuePair<string, SkillLineAgg>> src) =>
            src.Select(x => new SkillLinesApiResponse
            {
                LineName = x.Key,
                PlayersUsingThisLine = x.Value.PlayersUsingThisLine,
                UniqueSkillsCount = x.Value.UniqueSkillsCount,
                Skills = x.Value.Skills.ToList()
            }).ToList();

        public SkillLineReportEsologsResponse BuildSummary() => new()
        {
            TrialName = "All Zones",
            DdLinesModels = ToLines(dd),
            HealersLinesModels = ToLines(healer),
            TanksLinesModels = ToLines(tank)
        };
    }
}
