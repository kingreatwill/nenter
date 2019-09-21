using System;
using System.Linq;
using System.Data.Entity.Core.Objects;
namespace Nenter.Data.EntityFramework
{
    // https://github.com/aspnet/EntityFrameworkCore/issues/6482
    public static class XXXX
    {
        public static string ToSql<TEntity>(this IQueryable<TEntity> queryable)
            where TEntity : class
        {
            System.Data.Entity.Core.Objects.ObjectQuery
            string sql = ((System.Data.Objects.ObjectQuery)customerNames).ToTraceString();
 
            Console.WriteLine(sql);
        }

       
    }
}