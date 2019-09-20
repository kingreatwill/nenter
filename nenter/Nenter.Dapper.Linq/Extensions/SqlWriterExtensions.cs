using System.Linq.Expressions;
using Nenter.Dapper.Linq.Helpers;

namespace Nenter.Dapper.Linq.Extensions
{
    public static class SqlWriterExtensions
    {
        public static string GetPropertyNameWithIdentifierFromExpression<TData>(this ISqlWriter<TData> serverWriter, Expression expression)
        {
            var exp = expression.GetMemberExpression();
            if (!(exp is MemberExpression)) return string.Empty;
            var table = EntityTableCacheHelper.TryGetTable(((MemberExpression)exp).Expression.Type);
            var member = ((MemberExpression)exp).Member;
            return $"{table.Identifier}.{serverWriter.StartQuotationMark}{table.Columns[member.Name].ColumnName}{serverWriter.EndQuotationMark}";
        }
        
    }
}