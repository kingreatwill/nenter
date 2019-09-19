﻿using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nenter.Dapper.Linq.Extensions
{
    public static class DapperExtensions
    {
        public static IQueryable<T> Query<T>(this IDbConnection dbConnection, Expression<Func<T, bool>> where = null)
        {
            var query =  new Linq2Dapper<T>(dbConnection);
            return @where != null ? query.Where(@where) : query;
        }

        public static async Task<IQueryable<T>> QueryAsync<T>(this IDbConnection dbConnection, Expression<Func<T, bool>> expression = null)
        {
            return await Task.Run(() => Query(dbConnection, where: expression));
        }
    }
}
