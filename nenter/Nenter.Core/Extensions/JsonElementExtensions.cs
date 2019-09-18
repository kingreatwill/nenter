using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Nenter.Core.Extensions
{
    public static  class JsonElementExtensions
    {
        public static T Get<T>([NotNull] this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return default(T);
            }
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Object)
            {
                return JsonSerializer.Deserialize<T>(element.ToString());
            }
            else
            {
                return (T)Convert.ChangeType(element.ToString(), typeof(T));
            }
        }
    }
}