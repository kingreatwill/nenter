 using System.Linq;
 using System.Text;
 using Microsoft.VisualBasic;

 namespace Nenter.Dapper.Linq.Helpers
{
    public class SqlServerWriter<TData> : SqlWriter<TData>
    {
        public SqlServerWriter():base("[","]")
        {
        }

        protected override void SelectStatement()
        {
            var primaryTable = CacheHelper.TryGetTable<TData>();
            var selectTable = (SelectType != typeof(TData)) ? CacheHelper.TryGetTable(SelectType) : primaryTable;

            _selectStatement = new StringBuilder();

            _selectStatement.Append("SELECT ");

            if (TopCount > 0 && SkipCount == 0)
                _selectStatement.Append("TOP(" + TopCount + ") ");

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

            if (TopCount <= 0 || SkipCount <= 0) return;
            if (string.IsNullOrEmpty(_orderBy.ToString()))
            {
                //primaryTable.Columns
                var order = new StringBuilder();
                order.Append(Strings.Join(
                    primaryTable.Columns.Where(t => t.Value.PrimaryKey).Select(t =>
                            $"{primaryTable.Identifier}.{StartQuotationMark}{t.Value.ColumnName}{EndQuotationMark}")
                        .ToArray(),
                    ","
                ));
                order.Append(" ASC ");
                _orderBy.Insert(0, order);
            }
            _selectStatement.Append(" offset " + SkipCount + " rows fetch next "+TopCount+" rows only");
        }
    }
}
