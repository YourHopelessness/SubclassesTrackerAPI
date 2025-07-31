using FluentValidation;
using SubclassesTracker.Api.BackgroundQueue.Jobs;
using SubclassesTracker.Api.Models.Requests.Api;
using SubclassesTracker.Database.Entity;
using SubclassesTracker.Database.Repository;

namespace SubclassesTracker.Api.Middleware
{
    public class RequiredZonesForSpecificJobTypesAttribute : AbstractValidator<CreateNewJobApiRequest>
    {
        private readonly IBaseRepository<Zone> _repository;
        public RequiredZonesForSpecificJobTypesAttribute(IBaseRepository<Zone> repository)
        {
            _repository = repository;

            RuleFor(x => x.JobType).IsInEnum();

            When(x => x.JobType == JobsEnum.CollectDataForClassLines ||
                      x.JobType == JobsEnum.CollecctDataForRaces, () =>
                      {
                          RuleFor(x => x.CollectedZoneIds)
                          .Must(BeValidZones)
                          .WithMessage("One or more zone IDs are invalid");
                      });
        }

        private bool BeValidZones(List<int>? zoneIds)
        {
            return zoneIds == null 
                || _repository.GetList(x => zoneIds.Contains(x.Id)).Count() == zoneIds.Count;
        }
    }
}
