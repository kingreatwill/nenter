using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nenter.Data.PagedList;
using Nenter.Data.Dapper.SqlAdapter;

namespace Nenter.Data.Dapper
{
    public class DataRepository<TEntity> :IDataRepository<TEntity> where TEntity : class
    {
        
        public DataRepository(IDbConnection connection)
        {
            Connection = connection;
            SqlAdapter = new SqlServerAdapter<TEntity>();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DataRepository(IDbConnection connection, SqlProvider sqlProvider)
        {
            Connection = connection;
            SqlAdapter = sqlProvider switch
            {
                SqlProvider.SQLSERVER => (ISqlAdapter<TEntity>) new SqlServerAdapter<TEntity>(),
                SqlProvider.MySQL => new MySqlAdapter<TEntity>(),
                SqlProvider.PostgreSQL => new PostgresAdapter<TEntity>(),
                _ => throw new ArgumentException("sqlProvider")
            };
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DataRepository(IDbConnection connection, SqlAdapter<TEntity> sqlGenerator)
        {
            Connection = connection;
            SqlAdapter = sqlGenerator;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public DataRepository(IDbConnection connection, SqlAdapterConfig config)
        {
            Connection = connection;
            SqlAdapter = config.SqlProvider switch
            {
                SqlProvider.SQLSERVER => (ISqlAdapter<TEntity>) new SqlServerAdapter<TEntity>(config.UseQuotationMarks),
                SqlProvider.MySQL => new MySqlAdapter<TEntity>(config.UseQuotationMarks),
                SqlProvider.PostgreSQL => new PostgresAdapter<TEntity>(config.UseQuotationMarks),
                _ => throw new ArgumentException("sqlProvider")
            };
        }

        public IDbConnection Connection { get; }
        
        public IDbTransaction Transaction { get; }
        
        public ISqlAdapter<TEntity> SqlAdapter { get; }
        
        public virtual Task<bool> ChangeTableAsync(string table, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            }
            try
            {
                SqlAdapter.TableName = table;
                tcs.SetResult(true);
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }
            return tcs.Task;
        }

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetSelectFirst(predicate);
            return Connection.QueryFirstOrDefaultAsync<TEntity>(
                new CommandDefinition(queryResult.GetSql(), 
                    queryResult.Param, Transaction, 
                    null, null, CommandFlags.Buffered, 
                    cancellationToken)
                );
        }

        public Task<TEntity> FindAsync<TChild1, TChild2, TChild3, TChild4, TChild5, TChild6>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> tChild1,
            Expression<Func<TEntity, object>> tChild2, Expression<Func<TEntity, object>> tChild3, Expression<Func<TEntity, object>> tChild4, Expression<Func<TEntity, object>> tChild5, Expression<Func<TEntity, object>> tChild6,CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetSelectAll(predicate);
            return Connection.QueryAsync<TEntity>(
                new CommandDefinition(queryResult.GetSql(), 
                    queryResult.Param, Transaction, 
                    null, null, CommandFlags.Buffered, 
                    cancellationToken)
                );
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, Expression<Func<TEntity, object>> distinctField = null,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetCount(predicate, distinctField);
            return await Connection.QueryFirstOrDefaultAsync<int>(
                new CommandDefinition(queryResult.GetSql(), 
                    queryResult.Param, Transaction, 
                    null, null, CommandFlags.Buffered, 
                    cancellationToken)
                );
        }

        public virtual async Task<int> InsertAsync(TEntity instance,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetInsert(instance);
            if (!SqlAdapter.IsIdentity)
                return await Connection.ExecuteAsync(queryResult.GetSql(), instance, Transaction);
            var newId = (await Connection.QueryAsync<long>(
                new CommandDefinition(queryResult.GetSql(), 
                queryResult.Param, Transaction, 
                null, null, CommandFlags.Buffered, 
                cancellationToken)
                )).FirstOrDefault();
            return SetValue(newId, instance);

        }
        
        private int SetValue(long newId, TEntity instance)
        {
            if (newId > 0)
            {
                var newParsedId = Convert.ChangeType(newId, SqlAdapter.IdentitySqlProperty.PropertyInfo.PropertyType);
                SqlAdapter.IdentitySqlProperty.PropertyInfo.SetValue(instance, newParsedId);
            }
            return newId > 0 ?1 :0;
        }

        public virtual async Task<int> UpdateAsync(TEntity instance,CancellationToken cancellationToken = default)
        {
            var sqlQuery = SqlAdapter.GetUpdate(instance);
           return await Connection.ExecuteAsync(
               new CommandDefinition(
                   sqlQuery.GetSql(), 
                   sqlQuery.Param, 
                   Transaction, 
                   null, null, CommandFlags.Buffered, cancellationToken)
               ) ;
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predicate, TEntity instance,CancellationToken cancellationToken = default)
        {
            var sqlQuery = SqlAdapter.GetUpdate(predicate, instance);
            return await Connection.ExecuteAsync(
                       new CommandDefinition(
                           sqlQuery.GetSql(), 
                           sqlQuery.Param, 
                           Transaction, 
                           null, null, CommandFlags.Buffered, cancellationToken)
                       );
        }

        public virtual async Task<int> DeleteAsync(TEntity instance,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetDelete(instance);
            return await Connection.ExecuteAsync(
                new CommandDefinition(queryResult.GetSql(), queryResult.Param, Transaction, 
                    null, null, CommandFlags.Buffered, cancellationToken)
                );
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken = default)
        {
            var queryResult = SqlAdapter.GetDelete(predicate);
            return await Connection.ExecuteAsync(new CommandDefinition(queryResult.GetSql(), queryResult.Param, Transaction, null, null, CommandFlags.Buffered, cancellationToken));
        }

        public virtual async Task<int> BulkInsertAsync(IEnumerable<TEntity> instances,CancellationToken cancellationToken = default)
        {
            var enumerable = instances as TEntity[] ?? instances.ToArray();
            if (SqlAdapter.Config.SqlProvider == SqlProvider.SQLSERVER)
            {
                int count = 0;
                int totalInstances = enumerable.Count();

                var properties =
                    (SqlAdapter.IsIdentity
                        ? SqlAdapter.SqlProperties.Where(p => !p.PropertyName.Equals(SqlAdapter.IdentitySqlProperty.PropertyName, System.StringComparison.OrdinalIgnoreCase))
                        : SqlAdapter.SqlProperties).ToList();

                int exceededTimes = (int)Math.Ceiling(totalInstances * properties.Count / 2100d);
                if (exceededTimes > 1)
                {
                    int maxAllowedInstancesPerBatch = totalInstances / exceededTimes;

                    for (int i = 0; i <= exceededTimes; i++)
                    {
                        var items = enumerable.Skip(i * maxAllowedInstancesPerBatch).Take(maxAllowedInstancesPerBatch);
                        var msSqlQueryResult = SqlAdapter.GetBulkInsert(items);
                        count += await Connection.ExecuteAsync(new CommandDefinition(msSqlQueryResult.GetSql(), msSqlQueryResult.Param, Transaction, null, null, CommandFlags.Buffered, cancellationToken));
                    }
                    return count;
                }
            }
            var queryResult = SqlAdapter.GetBulkInsert(enumerable);
            return await Connection.ExecuteAsync(new CommandDefinition(queryResult.GetSql(), queryResult.Param, Transaction, null, null, CommandFlags.Buffered, cancellationToken));
        }

        public virtual async Task<int> BulkUpdateAsync(IEnumerable<TEntity> instances,CancellationToken cancellationToken = default)
        {
            var enumerable = instances as TEntity[] ?? instances.ToArray();
            if (SqlAdapter.Config.SqlProvider == SqlProvider.SQLSERVER)
            {
                int count = 0;
                int totalInstances = enumerable.Count();

                var properties = SqlAdapter.SqlProperties.ToList();

                int exceededTimes = (int)Math.Ceiling(totalInstances * properties.Count / 2100d);
                if (exceededTimes > 1)
                {
                    int maxAllowedInstancesPerBatch = totalInstances / exceededTimes;

                    for (int i = 0; i <= exceededTimes; i++)
                    {
                        var items = enumerable.Skip(i * maxAllowedInstancesPerBatch).Take(maxAllowedInstancesPerBatch);
                        var msSqlQueryResult = SqlAdapter.GetBulkUpdate(items);
                        count += await Connection.ExecuteAsync(new CommandDefinition(msSqlQueryResult.GetSql(), msSqlQueryResult.Param, Transaction, null, null, CommandFlags.Buffered, cancellationToken));
                    }
                    return count;
                }
            }
            var queryResult = SqlAdapter.GetBulkUpdate(enumerable);
            return await Connection.ExecuteAsync(new CommandDefinition(queryResult.GetSql(), queryResult.Param, Transaction, null, null, CommandFlags.Buffered, cancellationToken));
        }

        public IQueryable<TEntity> FromSql(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null,
            bool useTransaction = true,CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<TEntity>> GetPagedListAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, int pageIndex = 0, int pageSize = 10,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IPagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int pageIndex = 0, int pageSize = 10,
            CancellationToken cancellationToken = default(CancellationToken)) where TResult : class
        {
            throw new NotImplementedException();
        }
    }
}