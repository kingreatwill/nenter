 using System.Linq;
 using System.Text;

 namespace Nenter.Dapper.Linq.Helpers
{
    public class MySqlWriter<TData> : SqlWriter<TData>
    {
        public MySqlWriter():base("`","`")
        {
        }
        
        protected override void SelectStatement()
        {
            var primaryTable = EntityTableCacheHelper.TryGetTable<TData>();
            var selectTable = (SelectType != typeof(TData)) ? EntityTableCacheHelper.TryGetTable(SelectType) : primaryTable;

            _selectStatement = new StringBuilder();

            _selectStatement.Append("SELECT ");

            if (IsDistinct)
                _selectStatement.Append("DISTINCT ");

            if (IsCount)
            {
                _selectStatement.Append("COUNT(*) ");
            }
            else
            {
                for (int i = 0; i < selectTable.Columns.Count; i++)
                {
                    var x = selectTable.Columns.ElementAt(i);
                    _selectStatement.Append($"{selectTable.Identifier}.{StartQuotationMark}{x.Value.ColumnName}{EndQuotationMark}");

                    if ((i + 1) != selectTable.Columns.Count)
                        _selectStatement.Append(",");

                    _selectStatement.Append(" ");
                }

            }

            _selectStatement.Append($"FROM {StartQuotationMark}{primaryTable.Name}{EndQuotationMark} {primaryTable.Identifier}");
            _selectStatement.Append(WriteClause());

            if (TopCount <= 0) return;
            if (SkipCount > 0)
            {
                _selectStatement.Append(" LIMIT " + SkipCount+ " , " + TopCount + " ");
            }
            else
            {
                _selectStatement.Append(" LIMIT " + TopCount + " ");
            }
        }
    
    }
}
