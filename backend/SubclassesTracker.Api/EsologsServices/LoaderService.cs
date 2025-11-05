using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;
using SubclassesTracker.Models.Dto;
using SubclassesTracker.Models.Responses.Api;

namespace SubclassesTracker.Api.EsologsServices
{
    public interface ILoaderService
    {
        /// <summary>
        /// Load all trial zones
        /// </summary>
        /// <returns>List of zones and encounters</returns>
        Task<List<ZoneApiResponse>> LoadTrialZonesAsync(CancellationToken token);
        /// <summary>
        /// Load skills
        /// </summary>
        /// <returns>List of skills</returns>
        Task<Dictionary<int, SkillInfo>> LoadSkillsAsync(CancellationToken token);
        /// <summary>
        /// Load races passives
        /// </summary>
        /// <returns>Dict of racial and ability id</returns>
        Task<Dictionary<int, RacialPassivesInfo>> LoadRacialPassivesAsync(CancellationToken token);
    }

    /// <summary>
    /// Loader service
    /// </summary>
    public class LoaderService(
        IBaseRepository<SkillTreeEntry> skillsRepository,
        IBaseRepository<Zone> zoneRepository) : ILoaderService
    {
        public async Task<List<ZoneApiResponse>> LoadTrialZonesAsync(CancellationToken token)
        {
            return await zoneRepository
                .GetList(x => x.Type.Name == "Trial")
                .Select(x => new ZoneApiResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Encounters = x.Encounters
                        .Select(e => new EncounterApiResponse
                        {
                            Id = e.Id,
                            Name = e.Name,
                            ScoreCense = e.ScoreCense ?? 0
                        }).ToList()
                })
                .ToListAsync(token);
        }

        public async Task<Dictionary<int, SkillInfo>> LoadSkillsAsync(CancellationToken token)
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

        public async Task<Dictionary<int, RacialPassivesInfo>> LoadRacialPassivesAsync(CancellationToken token)
        {
            return await skillsRepository
                .GetList(x => x.SkillLine.LineType.Name == "Racial" && x.SkillType == "Passive")
                .Include(x => x.SkillLine)
                .ToDictionaryAsync(
                    k => k.AbilityId!,
                    v => new RacialPassivesInfo(
                        v.SkillName,
                        v.SkillLine.Name),
                    token);
        }
    }
}
