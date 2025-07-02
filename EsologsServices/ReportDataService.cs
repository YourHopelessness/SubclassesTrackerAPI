using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTrackerExtension.Models;
using System.Linq;

namespace SubclassesTrackerExtension.EsologsServices
{
    public interface IReportDataService
    {
        /// <summary>
        /// Retrieves skill lines for a specific zone (trial) based on the provided zone ID.
        /// </summary>
        /// <param name="zoneId">Id of zone</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        Task<List<SkillLineReportModel>> GetSkillLinesAsync(
            int zoneId,
            int difficulty,
            bool useScoreCense = false,
            CancellationToken token = new CancellationToken());
    }
    public class ReportDataService(
        IOptions<LinesConfig> options,
        IGetDataService dataService,
        IBaseRepository<SkillTreeEntry> skillsRepository,
        IBaseRepository<Encounter> encounterRepository) : IReportDataService
    {
        private sealed record PlayerKey(string Name, string TrialName, string Display, string Spec);
        private Dictionary<int, SkillInfo> skillsDict = [];
        private sealed record SkillInfo(string SkillName, string SkillLine, string SkillType);
       
        public async Task<List<SkillLineReportModel>> GetSkillLinesAsync(
            int zoneId,
            int difficulty,
            bool useScoreCense = false,
            CancellationToken token = new CancellationToken())
        {
            skillsDict = await skillsRepository
                .GetList(x => x.SkillLine.LineType.Name == "Class")
                .Include(x => x.SkillLine)
                .ToDictionaryAsync(k => 
                k.AbilityId ?? 0, 
                v => new SkillInfo(v.SkillName, v.SkillLine.Name, v.SkillType), 
                token);
            var reports = await dataService.GetAllReportsAndFights(zoneId, difficulty);
            var zones = await encounterRepository
                .GetList(x => x.Zone.Type.Name == "Trial")
                .Select(x => new ZoneModel()
                {
                    Id = x.Zone.Id,
                    Name = x.Zone.Name,
                    Encounters = x.Zone.Encounters
                        .Where(e => e.Type.Name == "Trial")
                        .Select(e => new EncounterModel
                        {
                            Id = e.Id,
                            Name = e.Name,
                            ScoreCense = e.ScoreCense ?? 0
                        }).ToList()
                })
                .ToListAsync(cancellationToken: token);

            var filtered = reports
                .Join(zones,
                        r => r.Zone.Id,
                        z => z.Id,
                        (r, z) => new
                        {
                            LogId = r.Code,
                            ZoneId = z.Id,
                            ZoneName = z.Name,
                            Fights = r.Fights.Where(f =>
                            {
                                // Filter fights based on trial score and encounter
                                if (!f.TrialScore.HasValue)
                                    return false;

                                // Check if the encounter exists in the zone
                                var enc = z.Encounters.FirstOrDefault(e => e.Id == f.EncounterId);
                                if (enc is null)
                                    return false;

                                // Check if the fight's trial score meets the criteria
                                return !useScoreCense || f.TrialScore.Value >= enc.ScoreCense;
                            }).ToList()
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
                let pd = playersDetail.First(p => p.LogId == rep.LogId)

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
                .Select(x => new PlayerRow(x.playerId, x.logId, [x.fightId], x.Role,
                                           x.TrialId, x.TrialName, x.Talents))
                .ToList();

            var needBuffs =
                rows.Where(r =>
                       r.Talents
                        .Where(t => skillsDict.ContainsKey(t.Id))
                        .Select(t => skillsDict[t.Id].SkillLine)
                        .Distinct()
                        .Count() < 3)
                    .ToList();

            if (needBuffs.Count > 0)
                rows.AddRange(await GetBuffsAsync(needBuffs));

            List<SkillLineReportModel> result =
                [.. rows.Where(x => x.TrialId == zoneId)
                    .Select(grp => new SkillLineReportModel
                    {
                        TrialId = grp.TrialId,
                        TrialName = grp.TrialName,
                        DdLinesModels = BuildLines(rows.Where(p => p.Role == "DPS"), skillsDict),
                        HealersLinesModels = BuildLines(rows.Where(p => p.Role == "Healer"), skillsDict),
                        TanksLinesModels = BuildLines(rows.Where(p => p.Role == "Tank"), skillsDict)
                    })];

            return result;
        }

        private static List<SkillLinesModel> BuildLines(
           IEnumerable<PlayerRow> players,
           IReadOnlyDictionary<int, SkillInfo> skillsDict)
        {
            // Get all skill lines
            var allLines = skillsDict.Values
                             .Select(s => s.SkillLine)
                             .Distinct()
                             .ToList();

            // Joining with the players' talents
            var flatten = players
               .SelectMany(p => p.Talents
                                 .Where(t => skillsDict.ContainsKey(t.Id))
                                 .Select(t => new
                                 {
                                     Player = p,
                                     SkillId = t.Id,
                                     Line = skillsDict[t.Id].SkillLine,
                                     SkillName = t.Name
                                 }))
               .ToList();

            // Grouping by all finded skills
            var realGroups = flatten
               .GroupBy(x => x.Line)
               .ToDictionary(g => g.Key, g => g.ToList());

            // Build the result
            var result = new List<SkillLinesModel>(allLines.Count);
            foreach (var line in allLines)
            {
                realGroups.TryGetValue(line, out var items);    // null -> no players used this line

                var skills = items?
                    .GroupBy(it => it.SkillId)
                    .Select(grp => new SkillModel
                    {
                        Id = grp.Key,
                        Name = grp.First().SkillName,
                        Type = skillsDict[grp.Key].SkillType
                    })
                    .ToList()
                    ?? [];

                var playersUsing = items?
                    .Select(it => it.Player)
                    .Distinct()
                    .Count() ?? 0;

                result.Add(new SkillLinesModel
                {
                    LineName = line,
                    UniqueSkillsCount = skills.Count,
                    PlayersUsingThisLine = playersUsing,
                    Skills = skills
                });
            }

            return result;
        }

        private async Task<List<PlayerRow>> GetBuffsAsync(List<PlayerRow> needBuffs)
        {
            foreach (var row in needBuffs)
            {
                var buffs = await dataService.GetPlayerBuffsAsync(
                                row.LogId, row.PlayerId, row.FightIds);

                var extra = buffs
                    .Where(b => skillsDict.ContainsKey(b.Id))
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