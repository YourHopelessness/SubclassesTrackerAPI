using SubclassesTracker.Api.Models.Dto;
using SubclassesTracker.Api.Models.Enums;
using SubclassesTracker.Api.Models.Responses.Api;
using SubclassesTracker.Api.Models.Responses.Esologs;

namespace SubclassesTracker.Api.EsologsServices.Reports
{
    public partial class ReportSubclassesDataService
    {
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

        /// <summary>
        /// Get lines stats
        /// </summary>
        private static List<PlayerRow> BuildDistinctBestFightRows(
            IReadOnlyCollection<FilteredReport> filteredReports,
            Dictionary<string, PlayerListResponse> playersByLog,
            bool filterNeeded = true)
        {
            // Collect all role-specific entries with their metadata
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

                // Add role-specific player fight entries
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

            // Pick the best entry per (GroupKey) — best per Name+Role+Spec
            var bestCandidates = raw
                .GroupBy(r => r.Key)
                .Select(g => g
                    .OrderByDescending(x => x.FightScore)
                    .First())
                .ToList();

            // Now deduplicate by (CharacterName, Role)
            var seenNameRolePairs = new HashSet<(string Name, PlayerRole Role)>();

            var best = new List<PlayerRow>();

            foreach (var entry in bestCandidates)
            {
                var key = (entry.Key.PlayerName, entry.Key.Role);

                if (seenNameRolePairs.Contains(key) 
                    && !entry.Key.PlayerName.Equals("nil", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                seenNameRolePairs.Add(key);

                best.Add(new PlayerRow(
                    entry.PlayerId,
                    entry.LogId,
                    [entry.FightId],
                    entry.Key.Role.ToString(),
                    entry.Key.TrialId,
                    entry.Key.TrialName,
                    entry.Talents,
                    entry.BaseClass));
            }

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
            var esoId = list.Dps?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? list.Healers?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? list.Tanks?.FirstOrDefault(p => p.Id == playerId)?.PlayerEsoId
                ?? "Anonymous";

            return esoId.Equals("nil", StringComparison.CurrentCultureIgnoreCase) ? "Anonymous" : esoId;
        }
    }
}
