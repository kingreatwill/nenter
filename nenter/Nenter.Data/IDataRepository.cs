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
        Task<bool> ChangeTableAsync(string table,CancellationToken cancellationToken = default);
        
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate = null);
        
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default);
        
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