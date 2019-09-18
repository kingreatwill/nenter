using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Nenter.Data.Extensions
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> ReflectionPropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public static PropertyInfo[] FindClassProperties(this Type objectType)
        {
            if (ReflectionPropertyCache.ContainsKey(objectType))
                return ReflectionPropertyCache[objectType];

            var propertyInfos = objectType.GetProperties()
                .OrderBy(p => p.GetCustomAttributes<ColumnAttribute>()
                    .Select(a => a.Order)
                    .DefaultIfEmpty(int.MaxValue)
                    .FirstOrDefault()).ToArray();

            ReflectionPropertyCache.TryAdd(objectType, propertyInfos);

            return propertyInfos;
        }
    }
}
