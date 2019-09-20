﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
 using Dapper;
 using Nenter.Dapper.Linq.Extensions;

 namespace Nenter.Dapper.Linq.Helpers
{
    public abstract class SqlWriter<TData> : ISqlWriter<TData>
    {
        protected StringBuilder _selectStatement;
        protected readonly StringBuilder _joinTable;
        protected readonly StringBuilder _whereClause;
        protected readonly StringBuilder _orderBy;

        
        //MSSQL private string startQuotationMark = "[", endQuotationMark = "]";
        //Mysql private string startQuotationMark = "`", endQuotationMark = "`";
        //PostgreSQL private string startQuotationMark = "\"", endQuotationMark = "\"";
        public string StartQuotationMark { get; }
        public string EndQuotationMark { get; }
        
        
        protected int _nextParameter;

        protected string _parameter
        {
            get { return $"ld__{_nextParameter += 1}"; }
        }
       
        public Type SelectType{ get; set; }
        public bool NotOperater{ get; set; }
        public int SkipCount { get; set; }
        public int TopCount{ get; set; }
        public bool IsDistinct{ get; set; }
        public bool IsCount { get; set; }

        public DynamicParameters Parameters { get; set; }

        public virtual string Sql
        {
            get
            {
                SelectStatement();
                return _selectStatement.ToString();
            }
        }
        
        public SqlWriter():this("","")
        {
        }

        public SqlWriter(string startQuotationMark = "", string endQuotationMark = "")
        {
            this.StartQuotationMark = startQuotationMark;
            this.EndQuotationMark = endQuotationMark;
            Parameters = new DynamicParameters();
            _joinTable = new StringBuilder();
            _whereClause = new StringBuilder();
            _orderBy = new StringBuilder();
            SelectType = typeof(TData);
            GetTypeProperties();
        }

        private EntityTable GetTypeProperties()
        {
           return EntityTableCacheHelper.ToEntityTable(typeof (TData));
        }

        protected virtual void SelectStatement()
        {
            var primaryTable = EntityTableCacheHelper.TryGetTable<TData>();
            var selectTable = (SelectType != typeof(TData)) ? EntityTableCacheHelper.TryGetTable(SelectType) : primaryTable;

            _selectStatement = new StringBuilder();

            _selectStatement.Append("SELECT ");

            if (TopCount > 0)
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
        }

        protected virtual string WriteClause()
        {
            var clause = string.Empty;

            // JOIN
            if (!string.IsNullOrEmpty(_joinTable.ToString()))
                clause += _joinTable;

            // WHERE
            if (!string.IsNullOrEmpty(_whereClause.ToString()))
                clause += " WHERE " + _whereClause;

            //ORDER BY
            if (!string.IsNullOrEmpty(_orderBy.ToString()))
                clause += " ORDER BY " + _orderBy;

            return clause;
        }

        public virtual void WriteOrder(string name, bool descending)
        {
            var order = new StringBuilder();
            order.Append(name);
            if (descending) order.Append(" DESC");
            if (!string.IsNullOrEmpty(_orderBy.ToString())) order.Append(", ");
            _orderBy.Insert(0, order);
        }

        public virtual void WriteJoin(string joinToTableName, string joinToTableIdentifier, string primaryJoinColumn, string secondaryJoinColumn)
        {
            _joinTable.Append(
                $" JOIN {StartQuotationMark}{joinToTableName}{EndQuotationMark} {joinToTableIdentifier} ON {primaryJoinColumn} = {secondaryJoinColumn}");
        }

        public virtual void Write(object value)
        {
            _whereClause.Append(value);
        }

        public virtual void Parameter(object val)
        {
            if (val == null)
            {
                Write("NULL");
                return;
            }

            var param = _parameter;
            Parameters.Add(param, val);

            Write("@" + param);
        }

        public virtual void AliasName(string aliasName)
        {
            Write(aliasName);
        }

        public virtual void ColumnName(string columnName)
        {
            Write(columnName);
        }

        public virtual void IsNull()
        {
            Write(" IS");
            if (!NotOperater)
                Write(" NOT");
            Write(" NULL");
            NotOperater = false;
        }

        public virtual void IsNullFunction()
        {
            Write("ISNULL");
        }

        public virtual void Like()
        {
            if (NotOperater)
                Write(" NOT");
            Write(" LIKE ");
            NotOperater = false;
        }

        public virtual void In()
        {
            if (NotOperater)
                Write(" NOT");
            Write(" IN ");
            NotOperater = false;
        }

        public virtual void Operator()
        {
            Write((NotOperater ? ExpressionType.NotEqual : ExpressionType.Equal).GetOperator());
            NotOperater = false;
        }

        public virtual void Boolean(bool op)
        {
            Write((op ? " <> " : " = ") + "0");
        }

        public virtual void OpenBrace()
        {
            Write("(");
        }

        public virtual void CloseBrace()
        {
            Write(")");
        }

        public virtual void WhiteSpace()
        {
            Write(" ");
        }

        public virtual void Delimiter()
        {
            Write(", ");
        }

        public virtual void LikePrefix()
        {
            Write("'%' + ");
        }

        public virtual void LikeSuffix()
        {
            Write("+ '%'");
        }

        public virtual void EmptyString()
        {
            Write("''");
        }
    }
}
