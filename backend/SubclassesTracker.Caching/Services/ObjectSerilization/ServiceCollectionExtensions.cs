using Microsoft.Extensions.DependencyInjection;

namespace SubclassesTracker.Caching.Services.ObjectSerilization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddObjectFlattening(this IServiceCollection services)
        {
            services.AddScoped<IObjectFlattener, ObjectFlattenerService>();
            services.AddScoped<IObjectUnflattener, ObjectUnflattenerService>();
            
            return services;
        }
    }
}
