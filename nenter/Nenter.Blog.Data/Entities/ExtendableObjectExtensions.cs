using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace Nenter.Blog.Data.Entities
{
    public static  class ExtendableObjectExtensions
    {

        public static T GetData<T>([NotNull] this IExtendableObject extendableObject, [NotNull] string name)
        {
            if (extendableObject.ExtensionData == null)
            {
                return default(T);
            }
            var json = JsonSerializer.Deserialize<Dictionary<string,dynamic>>(extendableObject.ExtensionData);
            var prop = json[name];
            if (prop == null)
            {
                return default(T);
            }
            try
            {
                return (T) Convert.ChangeType(prop, typeof(T));
            }
            catch (InvalidCastException) 
            {
                return default(T);
            }
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

            var json = JObject.Parse(extendableObject.ExtensionData);

            if (value == null || EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                if (json[name] != null)
                {
                    json.Remove(name);
                }
            }
            else if (TypeHelper.IsPrimitiveExtendedIncludingNullable(value.GetType()))
            {
                json[name] = new JValue(value);
            }
            else
            {
                json[name] = JToken.FromObject(value, jsonSerializer);
            }

            var data = json.ToString(Formatting.None);
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

            var json = JObject.Parse(extendableObject.ExtensionData);

            var token = json[name];
            if (token == null)
            {
                return false;
            }

            json.Remove(name);

            var data = json.ToString(Formatting.None);
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