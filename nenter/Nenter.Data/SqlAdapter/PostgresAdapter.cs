using System;
using System.Linq.Expressions;

namespace Nenter.Data.SqlAdapter
{
    public class PostgresAdapter<TEntity>:SqlAdapter<TEntity> where TEntity : class
    {
        public PostgresAdapter()
            : this(false)
        {
        }

        public PostgresAdapter(bool useQuotationMarks = false)
            : base(new SqlAdapterConfig
            {
                SqlProvider = SqlProvider.PostgreSQL,
                UseQuotationMarks = useQuotationMarks
            })
        {
        }
        
        public override SqlQuery  GetInsert(TEntity entity)
        {
            var query = base.GetInsert(entity);
            query.SqlBuilder.Append(" RETURNING " + IdentitySqlProperty.ColumnName);
            return query;
        }
        
        public override SqlQuery GetSelectById(object id, params Expression<Func<TEntity, object>>[] includes)
        {
            var sqlQuery = base.GetSelectById(id,includes);
            sqlQuery.SqlBuilder.Append("LIMIT 1");
            return sqlQuery;
        }
    }
}