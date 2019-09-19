 using System.Linq;
 using System.Text;

 namespace Nenter.Dapper.Linq.Helpers
{
    public class NpgSqlWriter<TData> : SqlWriter<TData>
    {
        public NpgSqlWriter():base("\"","\"")
        {
        }
        
        public override string Sql
        {
            get
            {
                SelectStatement();
                return _selectStatement.ToString();
            }
        }
        
        protected override void SelectStatement()
        {
            var primaryTable = CacheHelper.TryGetTable<TData>();
            var selectTable = (SelectType != typeof(TData)) ? CacheHelper.TryGetTable(SelectType) : primaryTable;

            _selectStatement = new StringBuilder();

            _selectStatement.Append("SELECT ");

            if (IsDistinct)
                _selectStatement.Append("DISTINCT ");

            for (int i = 0; i < selectTable.Columns.Count; i++)
            {
                var x = selectTable.Columns.ElementAt(i);
                _selectStatement.Append($"{selectTable.Identifier}.{startQuotationMark}{x.Value}{endQuotationMark}");

                if ((i + 1) != selectTable.Columns.Count)
                    _selectStatement.Append(",");

                _selectStatement.Append(" ");
            }

            _selectStatement.Append($"FROM {startQuotationMark}{primaryTable.Name}{endQuotationMark} {primaryTable.Identifier}");
            _selectStatement.Append(WriteClause());
            
            if (TopCount > 0)
                _selectStatement.Append(" LIMIT " + TopCount + " ");
        }
    
    }
}
