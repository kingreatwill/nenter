using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nenter.Data.PagedList;

namespace Nenter.Data
{
    public interface IDataRepository<TEntity> where TEntity : class
    {
        IDbContext GetContext();
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate, bool disableTracking = true);
        TEntity Find(params object[] keyValues);
        
        Task<bool> ChangeTableAsync(string table,CancellationToken cancellationToken = default);
        
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default);
        
        Task<TEntity> FindAsync<TChild1, TChild2, TChild3, TChild4, TChild5, TChild6>(Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>> tChild1,
            Expression<Func<TEntity, object>> tChild2,
            Expression<Func<TEntity, object>> tChild3,
            Expression<Func<TEntity, object>> tChild4,
            Expression<Func<TEntity, object>> tChild5,
            Expression<Func<TEntity, object>> tChild6,
            CancellationToken cancellationToken = default);
       
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            CancellationToken cancellationToken = default);
 
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> distinctField = null,CancellationToken cancellationToken = default);
      
        Task<int> InsertAsync(TEntity instance,CancellationToken cancellationToken = default);
      
        Task<int> UpdateAsync(TEntity instance,CancellationToken cancellationToken = default);
       
        Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, TEntity instance,CancellationToken cancellationToken = default);
       
        Task<int> DeleteAsync(TEntity instance,CancellationToken cancellationToken = default);
        
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default);

        Task<int> BulkInsertAsync(IEnumerable<TEntity> instances,CancellationToken cancellationToken = default);
        
        Task<int> BulkUpdateAsync(IEnumerable<TEntity> instances,CancellationToken cancellationToken = default);
        
        IQueryable<TEntity> FromSql(string sql, params object[] parameters);

        Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null,
            bool useTransaction = true,CancellationToken cancellationToken = default);
        
        Task<IPagedList<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int pageIndex = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default(CancellationToken));
        
        Task<IPagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int pageIndex = 0,
            int pageSize = 10,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class;
    }
}