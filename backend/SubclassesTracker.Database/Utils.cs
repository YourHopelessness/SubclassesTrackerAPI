using Microsoft.EntityFrameworkCore;

namespace SubclassesTracker.Database
{
    public static class QueryUtils
    {
        public static IQueryable<T> Detach<T>(this IQueryable<T> expression, bool noTracking = true)
            where T : class
        {
            if (noTracking)
            {
                return expression.AsNoTracking<T>();
            }

            return expression;
        }
    }
}
