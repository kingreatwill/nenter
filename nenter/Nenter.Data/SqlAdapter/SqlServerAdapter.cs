using System.Transactions;

namespace Nenter.Data.SqlAdapter
{
    public class SqlServerAdapter<TEntity>:SqlAdapter<TEntity> where TEntity : class
    {
        public SqlServerAdapter()
            : this(false)
        {
        }

        public SqlServerAdapter(bool useQuotationMarks = false)
            : base(new SqlAdapterConfig
            {
                SqlProvider = SqlProvider.SQLSERVER,
                UseQuotationMarks = useQuotationMarks
            })
        {
        }
        public override SqlQuery  GetInsert(TEntity entity)
        {
            var query = base.GetInsert(entity);
            query.SqlBuilder.Append(" SELECT SCOPE_IDENTITY() AS " + IdentitySqlProperty.ColumnName);
            return query;
        }
    }
}