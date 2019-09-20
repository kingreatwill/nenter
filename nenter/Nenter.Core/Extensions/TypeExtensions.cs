using System;

namespace Nenter.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }
    }
}
