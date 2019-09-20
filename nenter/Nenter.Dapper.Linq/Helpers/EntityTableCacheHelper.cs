﻿using System;
 using System.Collections.Concurrent;
using System.Collections.Generic;
 using System.ComponentModel.DataAnnotations;
 using System.ComponentModel.DataAnnotations.Schema;
 using System.Linq;
 using System.Reflection;

 namespace Nenter.Dapper.Linq.Helpers
{
    internal static class EntityTableCacheHelper
    {
        internal static int Size => TypeList.Count;

        private static readonly ConcurrentDictionary<Type, EntityTable> TypeList;

        static EntityTableCacheHelper()
        {
            if (TypeList == null)
                TypeList = new ConcurrentDictionary<Type, EntityTable>();
        }

        internal static bool HasCache<T>()
        {
            return HasCache(typeof (T));
        }

        internal static bool HasCache(Type type)
        {
            return TypeList.TryGetValue(type, out _);
        }

        internal static bool TryAddTable<T>(EntityTable entityTable)
        {
            return TryAddTable(typeof(T), entityTable);
        }

        internal static bool TryAddTable(Type type, EntityTable entityTable)
        {
            return TypeList.TryAdd(type, entityTable);
        }

        internal static EntityTable TryGetTable<T>()
        {
            return TryGetTable(typeof(T));
        }

        internal static EntityTable TryGetTable(Type type)
        {
            return !TypeList.TryGetValue(type, out var entityTable) ? new EntityTable() : entityTable;
        }

        internal static string TryGetIdentifier<T>()
        {
            return TryGetIdentifier(typeof(T));
        }

        internal static string TryGetIdentifier(Type type)
        {
            return TryGetTable(type).Identifier;
        }

        internal static SortedDictionary<string,EntityColumn> TryGetPropertyList<T>()
        {
            return TryGetPropertyList(typeof(T));
        }

        internal static SortedDictionary<string,EntityColumn> TryGetPropertyList(Type type)
        {
            return TryGetTable(type).Columns;
        }

        internal static string TryGetTableName<T>()
        {
            return TryGetTableName(typeof(T));
        }

        internal static string TryGetTableName(Type type)
        {
            return TryGetTable(type).Name;
        }
        
        internal static EntityTable ToEntityTable(Type type)
        {
            var table = TryGetTable(type);
            
            if (table.Name != null) return table; // have table in cache

            // get properties add to cache
            var properties = new SortedDictionary<string,EntityColumn>();
            type.GetProperties()
                .Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute)))
                .ToList()
                .ForEach(
                    x =>
                    {
                        var col = (ColumnAttribute)x.GetCustomAttribute(typeof(ColumnAttribute));
                        var dbgen = (DatabaseGeneratedAttribute)x.GetCustomAttribute(typeof(DatabaseGeneratedAttribute));
                        properties.Add(x.Name,new EntityColumn()
                        {
                            CSharpName = x.Name,
                            ColumnName = (col != null) ? col.Name : x.Name,
                            PrimaryKey = Attribute.IsDefined(x, typeof(KeyAttribute)),
                            ForeignKey = Attribute.IsDefined(x, typeof(ForeignKeyAttribute)),
                            GeneratedOption = dbgen==null?DatabaseGeneratedOption.None: dbgen.DatabaseGeneratedOption,
                        });
                    }
                );

            var attrib = (TableAttribute)type.GetCustomAttribute(typeof(TableAttribute));

            table = new EntityTable
            {
                Name = (attrib != null ? attrib.Name : type.Name),
                Columns = properties,
                Identifier = $"t{Size + 1}"
            };
            
            TryAddTable(type, table);

            return table;
        }

    }
}
