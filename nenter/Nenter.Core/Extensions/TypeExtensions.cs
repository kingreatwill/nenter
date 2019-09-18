using System;

namespace Nenter.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsBool(this Type type)
        {
            return type == typeof(bool);
        }
    }
}
