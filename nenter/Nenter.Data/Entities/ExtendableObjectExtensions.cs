using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Nenter.Core;
using Nenter.Core.Extensions;

namespace Nenter.Data.Entities
{
    public static  class ExtendableObjectExtensions
    {
        public static T GetData<T>([NotNull] this IExtendableObject extendableObject, [NotNull] string name)
        {
            if (extendableObject.ExtensionData == null)
            {
                return default(T);
            }
            var json = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(extendableObject.ExtensionData);
            var prop = json[name];
            return prop.Get<T>();
        }

        public static void SetData<T>([NotNull] this IExtendableObject extendableObject, [NotNull] string name, T value)
        {
            if (extendableObject.ExtensionData == null)
            {
                if (EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    return;
                }
                extendableObject.ExtensionData = "{}";
            }

            var json = JsonSerializer.Deserialize<Dictionary<string,Object>>(extendableObject.ExtensionData);

            if (value == null || EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                if (json[name] != null)
                {
                    json.Remove(name);
                }
            }
            
            json.Add(name,value);
            
            var data = JsonSerializer.Serialize(json);
            if (data == "{}")
            {
                data = null;
            }
            extendableObject.ExtensionData = data;
        }

        public static bool RemoveData([NotNull] this IExtendableObject extendableObject, string name)
        {
            if (extendableObject.ExtensionData == null)
            {
                return false;
            }

            var json = JsonSerializer.Deserialize<Dictionary<string,Object>>(extendableObject.ExtensionData);

            var token = json[name];
            if (token == null)
            {
                return false;
            }
            
            json.Remove(name);

            var data = JsonSerializer.Serialize(json);
            if (data == "{}")
            {
                data = null;
            }

            extendableObject.ExtensionData = data;

            return true;
        }

        //TODO: string[] GetExtendedPropertyNames(...)
    }
}