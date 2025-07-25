using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Api.Models.Dto;
using SubclassesTracker.Api.Models.Enums;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public partial class ReportDataService
    {
        private async Task<Dictionary<int, SkillInfo>> LoadSkillsAsync(CancellationToken token)
        {
            return await skillsRepository
                .GetList(x => x.SkillLine.LineType.Name == "Class")
                .Include(x => x.SkillLine)
                .Include(x => x.SkillLine.Icon)
                .Include(x => x.SkillLine.Class)
                .ToDictionaryAsync(
                    k => k.AbilityId!,
                    v => new SkillInfo(
                        v.SkillName, 
                        v.SkillLine.Name, 
                        v.SkillType, 
                        v.SkillLine?.Icon?.Url ?? null,
                        v.SkillLine?.Class?.Name ?? null),
                    token);
        }

        private async Task<List<PlayerRow>> GetBuffsAsync(
            List<PlayerRow> needBuffs, 
            CancellationToken token)
        {
            foreach (var row in needBuffs)
            {
                var buffs = await dataService.GetPlayerBuffsAsync(
                    row.LogId, 
                    row.PlayerId,
                    row.FightIds, 
                    token);

                var extra = buffs
                    .Where(b => skillsDict.ContainsKey(b.Id))
                    .Where(b => !row.Talents.Any(t => t.Id == b.Id))
                    .Select(b => new Talent(b.Id, b.Name));

                row.Talents.AddRange(extra);
            }

            return needBuffs;
        }

        private async Task<List<ZoneApiResponse>> LoadTrialZonesAsync(CancellationToken token)
        {
            return await encounterRepository
                .GetList(x => x.Zone.Type.Name == "Trial")
                .Select(x => new ZoneApiResponse
                {
                    Id = x.Zone.Id,
                    Name = x.Zone.Name,
                    Encounters = x.Zone.Encounters
                        .Where(e => e.Type.Name == "Trial")
                        .Select(e => new EncounterApiResponse
                        {
                            Id = e.Id,
                            Name = e.Name,
                            ScoreCense = e.ScoreCense ?? 0
                        }).ToList()
                })
                .ToListAsync(token);
        }

        /// <summary>
        /// Filter reports
        /// </summary>
        private static List<FilteredReport> FilterReports(
            List<ReportEsologsResponse> reports,
            List<ZoneApiResponse> zones,
            bool useScoreCense)
        {
            var zoneDict = zones.ToDictionary(z => z.Id);

            var filtered = new List<FilteredReport>();

            foreach (var report in reports)
            {
                if (!zoneDict.TryGetValue(report.Zone.Id, out var zone))
                    continue;

                var fights = report.Fights
                    .Where(f => f.TrialScore.HasValue)
                    .Where(f =>
                    {
                        var enc = zone.Encounters.FirstOrDefault(e => e.Id == f.EncounterId);
                        if (enc is null) return false;
                        return !useScoreCense || f.TrialScore!.Value >= enc.ScoreCense;
                    })
                    .ToList();

                if (fights.Count == 0)
                    continue;

                filtered.Add(new FilteredReport(report.Code, zone.Id, zone.Name, fights));
            }

            return filtered;
        }

        /// <summary>
        /// Load players reports
        /// </summary>
        private async Task<Dictionary<string, PlayerListResponse>> LoadPlayersForReportsAsync(
            List<FilteredReport> filteredReports,
            CancellationToken token)
        {
            var tasks = filteredReports
                .Select(async r =>
                {
                    var fightIds = r.Fights.Select(f => f.Id).ToList();
                    var players = await dataService.GetPlayersAsync(r.LogId, fightIds, token);
                    players.LogId = r.LogId;

                    return players;
                });

            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(p => p.LogId, p => p);
        }

        /// <summary>
        /// Get lines stats
        /// </summary>
        private static List<PlayerRow> BuildDistinctBestFightRows(
            IReadOnlyCollection<FilteredReport> filteredReports,
            Dictionary<string, PlayerListResponse> playersByLog,
            bool filterNeeded = true)
        {
            var raw = new List<(GroupKey Key, int PlayerId, string LogId, int FightId, int FightScore, List<Talent> Talents, string BaseClass)>();

            foreach (var report in filteredReports)
            {
                var players = playersByLog[report.LogId];

                foreach (var fight in report.Fights.Where(f => !filterNeeded || f.TrialScore.HasValue))
                {
                    AddRoleEntries(report, fight, players.Dps, PlayerRole.Dps);
                    AddRoleEntries(report, fight, players.Healers, PlayerRole.Healer);
                    AddRoleEntries(report, fight, players.Tanks, PlayerRole.Tank);
                }

                void AddRoleEntries(
                    FilteredReport rep,
                    FightEsologsResponse fight,
                    IEnumerable<PlayerEsologsResponse>? rolePlayers,
                    PlayerRole role)
                {
                    if (rolePlayers == null) return;

                    foreach (var p in rolePlayers)
                    {
                        var specKey = string.Join('|', p.Specs?.OrderBy(s => s) ?? Enumerable.Empty<string>());
                        var baseClass = p.BaseClass ?? "Unknown";
                        var talents = p.CombatInfo?.Talents?
                            .Select(t => new Talent(t.Id, t.Name))
                            .ToList() ?? [];

                        var key = new GroupKey(
                            p.Name,
                            rep.ZoneId,
                            rep.ZoneName,
                            p.PlayerEsoId,
                            specKey,
                            role);

                        raw.Add((key, p.Id, rep.LogId, fight.Id, fight.TrialScore ?? 0, talents, baseClass));
                    }
                }
            }

            var best = raw
                .GroupBy(r => r.Key)
                .Select(g => g
                    .OrderByDescending(x => x.FightScore)
                    .First())
                .Select(x => new PlayerRow(
                    x.PlayerId,
                    x.LogId,
                    [x.FightId],
                    x.Key.Role.ToString(),
                    x.Key.TrialId,
                    x.Key.TrialName,
                    x.Talents,
                    x.BaseClass))
                .ToList();

            return best;
        }

        /// <summary>
        /// Get missing Lines bu the buffs
        /// </summary>
        private async Task AddMissingBuffRowsAsync(
            List<PlayerRow> playerRows,
            CancellationToken token)
        {
            var lacking = playerRows
                .Where(r =>
                    r.Talents
                     .Where(t => skillsDict.ContainsKey(t.Id))
                     .Select(t => skillsDict[t.Id].SkillLine)
                     .Distinct()
                     .Count() < 3)
                .ToList();

            if (lacking.Count == 0)
                return;

            var buffs = await GetBuffsAsync(lacking, token);
            playerRows.AddRange(buffs);
        }

        private static List<SkillLinesApiResponse> BuildLines(
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
            var result = new List<SkillLinesApiResponse>(allLines.Count);
            foreach (var line in allLines)
            {
                realGroups.TryGetValue(line, out var items);    // null -> no players used this line

                var skills = items?
                    .GroupBy(it => it.SkillId)
                    .Select(grp => new SkillApiResponse
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

                result.Add(new SkillLinesApiResponse
                {
                    LineName = line,
                    UniqueSkillsCount = skills.Count,
                    PlayersUsingThisLine = playersUsing,
                    Skills = skills
                });
            }

            return result;
        }

        private static string GetPlayerCharacterName(int playerId, PlayerListResponse list)
        {
            return list.Dps?.FirstOrDefault(p => p.Id == playerId)?.Name
                ?? list.Healers?.FirstOrDefault(p => p.Id == playerId)?.Name
                ?? list.Tanks?.FirstOrDefault(p => p.Id == playerId)?.Name
                ?? "Anonymous";
        }

        private static string GetPlayerEsoId(int playerId, PlayerListResponse list)
        {
            return list.Dps?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? list.Healers?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? list.Tanks?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? string.Empty;
        }
    }
}
