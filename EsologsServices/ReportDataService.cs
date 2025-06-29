using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Options;
using SubclassesTrackerExtension.ExcelServices;
using SubclassesTrackerExtension.Models;
using System.Linq;
using static SubclassesTrackerExtension.ExcelServices.ExcelParserService;

namespace SubclassesTrackerExtension.EsologsServices
{
    public interface IReportDataService
    {
        Task<List<SkillLineReportModel>> GetSkillLinesAsync(int zoneId);
    }
    public class ReportDataService(
        IOptions<LinesConfig> options,
        IGetDataService dataService) : IReportDataService
    {
        private sealed record PlayerKey(string Name, string TrialName, string Display, string Spec);
        private readonly Dictionary<string, SkillInfo> skillsDict = LoadSkills(options.Value.LinesSkillExcel);
        private static List<SkillLinesModel> BuildLines(
            IEnumerable<PlayerRow> players,
            IReadOnlyDictionary<string, SkillInfo> skillsDict)
        {
            var flatten = players
                    .SelectMany(p => p.Talents
                         .Where(t => skillsDict.ContainsKey(t.Name))
                         .Select(t => new
                         {
                             Player = p,
                             SkillId = t.Id,
                             SkillName = t.Name,
                             Line = skillsDict[t.Name].Line
                         }));

            var lineGroups = flatten.GroupBy(x => x.Line);

            return lineGroups.Select(g =>
            {
                var skills = g.GroupBy(x => x.SkillId)
                              .Select(grp => new SkillModel
                              {
                                  Id = grp.Key,
                                  Name = grp.First().SkillName
                              })
                              .ToList();

                int playersUsing = g.Select(x => x.Player).Distinct().Count();

                return new SkillLinesModel
                {
                    LineName = g.Key,
                    UniqueSkillsCount = skills.Count,
                    PlayersUsingThisLine = playersUsing,
                    Skills = skills
                };
            }).ToList();
        }

        public async Task<List<SkillLineReportModel>> GetSkillLinesAsync(int zoneId)
        {
            var reports = await dataService.GetAllReportsAndFights(zoneId);
            var zones = await dataService.GetAllZonesAndEncountersAsync();

            var filtered = reports
                .Join(zones,
                        r => r.Zone.Id,
                        z => z.Id,
                        (r, z) => new
                        {
                            LogId = r.Code,
                            ZoneId = z.Id,
                            ZoneName = z.Name,
                            Fights = r.Fights
                            .Where(f => z.Encounters.Select(e => e.Name)
                                    .Contains(f.Name) &&
                                    f.TrialScore.HasValue)
                            .ToList()
                        })
                .Where(x => x.Fights.Count != 0)
                .ToList();

            var playersDetail = new List<PlayerListModel>();
            foreach (var report in filtered)
            {
                var players = await dataService.GetPlayersAsync(report.LogId, [.. report.Fights.Where(f => f.TrialScore != null).Select(f => f.Id)]);
                players.LogId = report.LogId;
                playersDetail.Add(players);
            }

            var rows = (
                from rep in filtered
                from fight in rep.Fights.Where(f => f.TrialScore.HasValue)
                let pd = playersDetail.Single(p => p.LogId == rep.LogId)

                from tuple in new[] { ("DPS", pd.Dps), ("Healer", pd.Healers), ("Tank", pd.Tanks) }
                let role = tuple.Item1
                from player in tuple.Item2 ?? Enumerable.Empty<PlayerModel>()

                let specKey = string.Join('|', player.Specs.OrderBy(s => s))

                select new
                {
                    Key = (player.Name, rep.ZoneName, player.PlayerEsoId, specKey, role),
                    playerId = player.Id,
                    logId = rep.LogId,
                    fightId = fight.Id,

                    Role = role,
                    TrialId = rep.ZoneId,
                    TrialName = rep.ZoneName,
                    FightScore = fight.TrialScore.GetValueOrDefault(),
                    Talents = player.CombatInfo.Talents
                                       .Select(t => new Talent(t.Id, t.Name))
                                       .ToList()
                })
                .GroupBy(x => x.Key)
                .Select(g => g.OrderByDescending(x => x.FightScore).First())
                .Select(x => new PlayerRow(x.playerId, x.logId, new[] { x.fightId }, x.Role,
                                           x.TrialId, x.TrialName, x.Talents))
                .ToList();

            var needBuffs = 
                rows.Where(r =>
                       r.Talents
                        .Where(t => skillsDict.ContainsKey(t.Name))
                        .Select(t => skillsDict[t.Name].Line)
                        .Distinct()
                        .Count() < 3)
                    .ToList();

            if (needBuffs.Count > 0)
                rows.AddRange(await GetBuffsAsync(needBuffs));

            List<SkillLineReportModel> result =
                rows.Where(x => x.TrialId == zoneId)
                    .Select(grp => new SkillLineReportModel
                    {
                        TrialId = grp.TrialId,
                        TrialName = grp.TrialName,
                        DdLinesModels = BuildLines(rows.Where(p => p.Role == "DPS"), skillsDict),
                        HealersLinesModels = BuildLines(rows.Where(p => p.Role == "Healer"), skillsDict),
                        TanksLinesModels = BuildLines(rows.Where(p => p.Role == "Tank"), skillsDict)
                    }).ToList();

            return result;
        }

        private async Task<List<PlayerRow>> GetBuffsAsync(List<PlayerRow> needBuffs)
        {
            foreach (var row in needBuffs)
            {
                var buffs = await dataService.GetPlayerBuffsAsync(
                                row.LogId, row.PlayerId, row.FightIds);

                var extra = buffs
                    .Where(b => skillsDict.ContainsKey(b.Name))
                    .Where(b => !row.Talents.Any(t => t.Id == b.Id))
                    .Select(b => new Talent(b.Id, b.Name));

                row.Talents.AddRange(extra);
            }

            return needBuffs;
        }

        /// <summary>
        /// Represents a skill line report model for a specific trial.
        /// </summary>
        public sealed record Talent(int Id, string Name);

        /// <summary>
        /// Represents a row of player data in the report.
        /// </summary>
        public sealed record PlayerRow(
            int PlayerId,
            string LogId,
            IReadOnlyList<int> FightIds,
            string Role,
            int TrialId,
            string TrialName,
            List<Talent> Talents);
    }
}