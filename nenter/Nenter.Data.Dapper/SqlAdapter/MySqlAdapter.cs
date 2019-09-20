using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Nenter.Data.Dapper.SqlAdapter
{
    public class MySqlAdapter<TEntity>:SqlAdapter<TEntity> where TEntity : class
    {
        
        public MySqlAdapter()
            : this(false)
        {
        }

        public MySqlAdapter(bool useQuotationMarks = false)
            : base(new SqlAdapterConfig
            {
                SqlProvider = SqlProvider.MySQL,
                UseQuotationMarks = useQuotationMarks
            })
        {
        }
        
        public override SqlQuery  GetInsert(TEntity entity)
        {
            var query = base.GetInsert(entity);
            query.SqlBuilder.Append("; SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS " + IdentitySqlProperty.ColumnName);
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