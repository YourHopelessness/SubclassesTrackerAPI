using System.Linq.Expressions;
using System.Reflection;

namespace SubclassesTracker.Caching.Parquet
{
    public interface IParquetColumn<T>
    {
        string Name { get; }
        Type ColumnType { get; }
        Func<T, object?> Getter { get; }
    }


    public sealed class ParquetColumn<T>(string name, Type type, Func<T, object?> getter) : IParquetColumn<T>
    {
        public string Name { get; } = name;
        public Type ColumnType { get; } = type;
        public Func<T, object?> Getter { get; } = getter;

        public static ParquetColumn<T> FromProperty(PropertyInfo prop)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var access = Expression.Property(param, prop);
            var cast = Expression.Convert(access, typeof(object));
            var lambda = Expression.Lambda<Func<T, object?>>(cast, param).Compile();

            return new ParquetColumn<T>(prop.Name, prop.PropertyType, lambda);
        }
    }
}
