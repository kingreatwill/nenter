﻿using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
 using Dapper;
 using Nenter.Dapper.Linq.Helpers;
 namespace Nenter.Dapper.Linq
{
    internal class QueryProvider<TData> : IQueryProvider
    {
        private readonly IDbConnection _connection;
        private readonly QueryBuilder<TData> _qb; 

        public QueryProvider(IDbConnection connection)
        {
            _connection = connection;
            _qb = connection.GetType().Name switch
            {
                "MySqlConnection" => new QueryBuilder<TData>(new MySqlWriter<TData>()),
                "NpgsqlConnection" => new QueryBuilder<TData>(new NpgSqlWriter<TData>()),
                _ => new QueryBuilder<TData>(new SqlServerWriter<TData>())
            };
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(Linq2Dapper<TData>).MakeGenericType(elementType), this, expression);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        // Queryable's collection-returning standard query operators call this method. 
        public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
        {
            return new Linq2Dapper<TResult>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Query(expression);
        }

        // Queryable's "single value" standard query operators call this method.
        public TResult Execute<TResult>(Expression expression)
        {
            var t = Query(expression, typeof(IEnumerable).IsAssignableFrom(typeof(TResult)));
            if (t == null)
            {
                return default(TResult);
            }
            return (TResult)t;
        }
        
        // Executes the expression tree that is passed to it. 
        private object Query(Expression expression, bool isEnumerable = false)
        {
            try
            {
                if (_connection.State != ConnectionState.Open) _connection.Open();

                _qb.Evaluate(expression);

                if (expression.Type == typeof(int))
                {
                    return  _connection.Query<int>(_qb.Sql, _qb.Parameters).ElementAt(0);
                }
                else  if (expression.Type == typeof(long))
                {
                    return  _connection.Query<long>(_qb.Sql, _qb.Parameters).ElementAt(0);
                }

                var data = _connection.Query<TData>(_qb.Sql, _qb.Parameters);

                if (isEnumerable) return data;
                
                var enumerable = data as TData[] ?? data.ToArray();
                return !enumerable.Any() ? default(TData) : enumerable.ElementAt(0);
            }
            finally
            {
                _connection.Close();
            }
        }

    }
}
