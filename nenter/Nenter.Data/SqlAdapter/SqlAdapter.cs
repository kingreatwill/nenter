using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Nenter.Core.Extensions;
using Nenter.Data.Attributes;
using Nenter.Data.Attributes.Joins;
using Nenter.Data.Attributes.LogicalDelete;
using Nenter.Data.SqlAdapter.QueryExpressions;
using TypeExtensions = Nenter.Data.Extensions.TypeExtensions;

namespace Nenter.Data.SqlAdapter
{
    public abstract class SqlAdapter<TEntity> : ISqlAdapter<TEntity>
        where TEntity : class
    {
        
        public SqlAdapter()
            : this(new SqlAdapterConfig
            {
                SqlProvider = SqlProvider.SQLSERVER,
                UseQuotationMarks = false
            })
        {
        }

        public SqlAdapter(SqlProvider sqlProvider, bool useQuotationMarks = false)
            : this(new SqlAdapterConfig
            {
                SqlProvider = sqlProvider,
                UseQuotationMarks = useQuotationMarks
            })
        {
        }
        
        public SqlAdapter(SqlAdapterConfig sqlAdapterConfig)
        {
            // Order is important
            InitProperties();
            InitConfig(sqlAdapterConfig);
            InitLogicalDeleted();
        }

        #region private
        private enum QueryType
        {
            Select,
            Delete,
            Update
        }
         private void InitProperties()
        {
            var entityType = typeof(TEntity);
            var entityTypeInfo = entityType.GetTypeInfo();
            var tableAttribute = entityTypeInfo.GetCustomAttribute<TableAttribute>();

            TableName = tableAttribute != null ? tableAttribute.Name : entityTypeInfo.Name;
            TableSchema = tableAttribute != null ? tableAttribute.Schema : string.Empty;

            AllProperties = TypeExtensions.FindClassProperties(entityType).Where(q => q.CanWrite).ToArray();

            var props = AllProperties.Where(ExpressionHelper.GetPrimitivePropertiesPredicate()).ToArray();

            var joinProperties = AllProperties.Where(p => p.GetCustomAttributes<JoinAttributeBase>().Any()).ToArray();

            SqlJoinProperties = GetJoinPropertyMetadata(joinProperties);

            // Filter the non stored properties
            SqlProperties = props.Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any()).Select(p => new SqlPropertyMetadata(p)).ToArray();

            // Filter key properties
            KeySqlProperties = props.Where(p => p.GetCustomAttributes<KeyAttribute>().Any()).Select(p => new SqlPropertyMetadata(p)).ToArray();

            // Use identity as key pattern
            var identityProperty = props.FirstOrDefault(p => p.GetCustomAttributes<IdentityAttribute>().Any());
            IdentitySqlProperty = identityProperty != null ? new SqlPropertyMetadata(identityProperty) : null;

            var dateChangedProperty = props.FirstOrDefault(p => p.GetCustomAttributes<UpdatedAtAttribute>().Count() == 1);
            if (dateChangedProperty != null && (dateChangedProperty.PropertyType == typeof(DateTime) || dateChangedProperty.PropertyType == typeof(DateTime?)))
            {
                UpdatedAtProperty = props.FirstOrDefault(p => p.GetCustomAttributes<UpdatedAtAttribute>().Any());
                UpdatedAtPropertyMetadata = new SqlPropertyMetadata(UpdatedAtProperty);
            }
        }
         
         private void InitConfig(SqlAdapterConfig sqlAdapterConfig)
        {
            Config = sqlAdapterConfig;

            if (Config.UseQuotationMarks)
            {
                switch (Config.SqlProvider)
                {
                    case SqlProvider.SQLSERVER:
                        TableName = GetTableNameWithSchemaPrefix(TableName, TableSchema, "[", "]");

                        foreach (var propertyMetadata in SqlProperties)
                            propertyMetadata.ColumnName = "[" + propertyMetadata.ColumnName + "]";

                        foreach (var propertyMetadata in KeySqlProperties)
                            propertyMetadata.ColumnName = "[" + propertyMetadata.ColumnName + "]";

                        foreach (var propertyMetadata in SqlJoinProperties)
                        {
                            propertyMetadata.TableName = GetTableNameWithSchemaPrefix(propertyMetadata.TableName, propertyMetadata.TableSchema, "[", "]");
                            propertyMetadata.ColumnName = "[" + propertyMetadata.ColumnName + "]";
                            propertyMetadata.TableAlias = "[" + propertyMetadata.TableAlias + "]";
                        }

                        if (IdentitySqlProperty != null)
                            IdentitySqlProperty.ColumnName = "[" + IdentitySqlProperty.ColumnName + "]";

                        break;

                    case SqlProvider.MySQL:
                        TableName = GetTableNameWithSchemaPrefix(TableName, TableSchema, "`", "`");

                        foreach (var propertyMetadata in SqlProperties)
                            propertyMetadata.ColumnName = "`" + propertyMetadata.ColumnName + "`";

                        foreach (var propertyMetadata in KeySqlProperties)
                            propertyMetadata.ColumnName = "`" + propertyMetadata.ColumnName + "`";

                        foreach (var propertyMetadata in SqlJoinProperties)
                        {
                            propertyMetadata.TableName = GetTableNameWithSchemaPrefix(propertyMetadata.TableName, propertyMetadata.TableSchema, "`", "`");
                            propertyMetadata.ColumnName = "`" + propertyMetadata.ColumnName + "`";
                            propertyMetadata.TableAlias = "`" + propertyMetadata.TableAlias + "`";
                        }

                        if (IdentitySqlProperty != null)
                            IdentitySqlProperty.ColumnName = "`" + IdentitySqlProperty.ColumnName + "`";

                        break;

                    case SqlProvider.PostgreSQL:
                        TableName = GetTableNameWithSchemaPrefix(TableName, TableSchema, "\"", "\"");

                        foreach (var propertyMetadata in SqlProperties)
                            propertyMetadata.ColumnName = "\"" + propertyMetadata.ColumnName + "\"";

                        foreach (var propertyMetadata in KeySqlProperties)
                            propertyMetadata.ColumnName = "\"" + propertyMetadata.ColumnName + "\"";

                        foreach (var propertyMetadata in SqlJoinProperties)
                        {
                            propertyMetadata.TableName = GetTableNameWithSchemaPrefix(propertyMetadata.TableName, propertyMetadata.TableSchema, "\"", "\"");
                            propertyMetadata.ColumnName = "\"" + propertyMetadata.ColumnName + "\"";
                            propertyMetadata.TableAlias = "\"" + propertyMetadata.TableAlias + "\"";
                        }

                        if (IdentitySqlProperty != null)
                            IdentitySqlProperty.ColumnName = "\"" + IdentitySqlProperty.ColumnName + "\"";

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(Config.SqlProvider));
                }
            }
            else
            {
                TableName = GetTableNameWithSchemaPrefix(TableName, TableSchema);
                foreach (var propertyMetadata in SqlJoinProperties)
                    propertyMetadata.TableName = GetTableNameWithSchemaPrefix(propertyMetadata.TableName, propertyMetadata.TableSchema);
            }
        }
         
         private void InitLogicalDeleted()
         {
             var statusProperty =
                 SqlProperties.FirstOrDefault(x => x.PropertyInfo.GetCustomAttributes<StatusAttribute>().Any());

             if (statusProperty == null)
                 return;
             StatusPropertyName = statusProperty.ColumnName;

             if (statusProperty.PropertyInfo.PropertyType.IsBoolean())
             {
                 var deleteProperty = AllProperties.FirstOrDefault(p => p.GetCustomAttributes<DeletedAttribute>().Any());
                 if (deleteProperty == null)
                     return;

                 LogicalDelete = true;
                 LogicalDeleteValue = 1; // true
             }
             else if (statusProperty.PropertyInfo.PropertyType.IsEnum)
             {
                 var deleteOption = statusProperty.PropertyInfo.PropertyType.GetFields().FirstOrDefault(f => f.GetCustomAttribute<DeletedAttribute>() != null);

                 if (deleteOption == null)
                     return;

                 var enumValue = Enum.Parse(statusProperty.PropertyInfo.PropertyType, deleteOption.Name);
                 LogicalDeleteValue = Convert.ChangeType(enumValue, Enum.GetUnderlyingType(statusProperty.PropertyInfo.PropertyType));

                 LogicalDelete = true;
             }
         }
         
         
        /// <summary>
        ///     Get join/nested properties
        /// </summary>
        /// <returns></returns>
        private static SqlJoinPropertyMetadata[] GetJoinPropertyMetadata(PropertyInfo[] joinPropertiesInfo)
        {
            // Filter and get only non collection nested properties
            var singleJoinTypes = joinPropertiesInfo.Where(p => !p.PropertyType.IsConstructedGenericType).ToArray();

            var joinPropertyMetadatas = new List<SqlJoinPropertyMetadata>();

            foreach (var propertyInfo in singleJoinTypes)
            {
                var joinInnerProperties = propertyInfo.PropertyType.GetProperties().Where(q => q.CanWrite)
                    .Where(ExpressionHelper.GetPrimitivePropertiesPredicate()).ToArray();
                joinPropertyMetadatas.AddRange(joinInnerProperties.Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any())
                    .Select(p => new SqlJoinPropertyMetadata(propertyInfo, p)).ToArray());
            }

            return joinPropertyMetadatas.ToArray();
        }

        private static string GetTableNameWithSchemaPrefix(string tableName, string tableSchema, string startQuotationMark = "", string endQuotationMark = "")
        {
            return !string.IsNullOrEmpty(tableSchema)
                ? startQuotationMark + tableSchema + endQuotationMark + "." + startQuotationMark + tableName + endQuotationMark
                : startQuotationMark + tableName + endQuotationMark;
        }
        
         private void AppendWherePredicateQuery(SqlQuery sqlQuery, Expression<Func<TEntity, bool>> predicate, QueryType queryType)
        {
            IDictionary<string, object> dictionaryParams = new Dictionary<string, object>();

            if (predicate != null)
            {
                // WHERE
                var queryProperties = GetQueryProperties(predicate.Body);

                sqlQuery.SqlBuilder.Append("WHERE ");

                var qLevel = 0;
                var sqlBuilder = new StringBuilder();
                var conditions = new List<KeyValuePair<string, object>>();
                BuildQuerySql(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

                dictionaryParams.AddRange(conditions);

                if (LogicalDelete && queryType == QueryType.Select)
                    sqlQuery.SqlBuilder.AppendFormat("({3}) AND {0}.{1} != {2} ", TableName, StatusPropertyName, LogicalDeleteValue, sqlBuilder);
                else
                    sqlQuery.SqlBuilder.AppendFormat("{0} ", sqlBuilder);
            }
            else
            {
                if (LogicalDelete && queryType == QueryType.Select)
                    sqlQuery.SqlBuilder.AppendFormat("WHERE {0}.{1} != {2} ", TableName, StatusPropertyName, LogicalDeleteValue);
            }

            if (LogicalDelete && HasUpdatedAt && queryType == QueryType.Delete)
                dictionaryParams.Add(UpdatedAtPropertyMetadata.ColumnName, DateTime.UtcNow);

            sqlQuery.SetParam(dictionaryParams);
        }
        
        /// <summary>
        /// Build the final `query statement and parameters`
        /// </summary>
        /// <param name="queryProperties"></param>
        /// <param name="sqlBuilder"></param>
        /// <param name="conditions"></param>
        /// <param name="qLevel">Parameters of the ranking</param>
        /// <remarks>
        /// Support `group conditions` syntax
        /// </remarks>
        private void BuildQuerySql(IList<QueryExpression> queryProperties,
           ref StringBuilder sqlBuilder, ref List<KeyValuePair<string, object>> conditions, ref int qLevel)
        {
            foreach (var expr in queryProperties)
            {
                if (!string.IsNullOrEmpty(expr.LinkingOperator))
                {
                    if (sqlBuilder.Length > 0)
                        sqlBuilder.Append(" ");
                    
                    sqlBuilder
                        .Append(expr.LinkingOperator)
                        .Append(" ");
                }

                switch (expr)
                {
                    case QueryParameterExpression qpExpr:
                        var tableName = TableName;
                        string columnName;
                        if (qpExpr.NestedProperty)
                        {
                            var joinProperty = SqlJoinProperties.First(x => x.PropertyName == qpExpr.PropertyName);
                            tableName = joinProperty.TableAlias;
                            columnName = joinProperty.ColumnName;
                        }
                        else
                        {
                            columnName = SqlProperties.First(x => x.PropertyName == qpExpr.PropertyName).ColumnName;
                        }

                        if (qpExpr.PropertyValue == null)
                        {
                            sqlBuilder.AppendFormat("{0}.{1} {2} NULL", tableName, columnName, qpExpr.QueryOperator == "=" ? "IS" : "IS NOT");
                        }
                        else
                        {
                            var vKey = $"{qpExpr.PropertyName}_p{qLevel}"; //Handle multiple uses of a field
                            
                            sqlBuilder.AppendFormat("{0}.{1} {2} @{3}", tableName, columnName, qpExpr.QueryOperator, vKey);
                            conditions.Add(new KeyValuePair<string, object>(vKey, qpExpr.PropertyValue));
                        }

                        qLevel++;
                        break;

                    case QueryBinaryExpression qbExpr:
                        var nSqlBuilder = new StringBuilder();
                        var nConditions = new List<KeyValuePair<string, object>>();
                        BuildQuerySql(qbExpr.Nodes, ref nSqlBuilder, ref nConditions, ref qLevel);

                        if (qbExpr.Nodes.Count == 1) //Handle `grouping brackets`
                            sqlBuilder.Append(nSqlBuilder);
                        else
                            sqlBuilder.AppendFormat("({0})", nSqlBuilder);

                        conditions.AddRange(nConditions);
                        break;
                }
            }
        }
        
          private QueryExpression GetQueryProperties(Expression expr, ExpressionType linkingType)
        {
            var isNotUnary = false;
            if (expr is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Not && unaryExpression.Operand is MethodCallExpression)
                {
                    expr = unaryExpression.Operand;
                    isNotUnary = true;
                }
            }

            if (expr is MethodCallExpression methodCallExpression)
            {
                var methodName = methodCallExpression.Method.Name;
                var exprObj = methodCallExpression.Object;
            MethodLabel:
                switch (methodName)
                {
                    case "Contains":
                        {
                            if (exprObj != null
                                && exprObj.NodeType == ExpressionType.MemberAccess
                                && exprObj.Type == typeof(string))
                            {
                                methodName = "StringContains";
                                goto MethodLabel;
                            }

                            var propertyName = ExpressionHelper.GetPropertyNamePath(methodCallExpression, out var isNested);

                            if (!SqlProperties.Select(x => x.PropertyName).Contains(propertyName) &&
                                !SqlJoinProperties.Select(x => x.PropertyName).Contains(propertyName))
                                throw new NotSupportedException("predicate can't parse");

                            var propertyValue = ExpressionHelper.GetValuesFromCollection(methodCallExpression);
                            var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                            var link = ExpressionHelper.GetSqlOperator(linkingType);
                            return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                        }
                    case "StringContains":
                    case "StartsWith":
                    case "EndsWith":
                        {
                            if (exprObj == null
                                || exprObj.NodeType != ExpressionType.MemberAccess
                                || exprObj.Type != typeof(string))
                            {
                                goto default;
                            }

                            var propertyName = ExpressionHelper.GetPropertyNamePath(exprObj, out bool isNested);

                            if (!SqlProperties.Select(x => x.PropertyName).Contains(propertyName) &&
                                !SqlJoinProperties.Select(x => x.PropertyName).Contains(propertyName))
                                throw new NotSupportedException("predicate can't parse");

                            var propertyValue = ExpressionHelper.GetValuesFromStringMethod(methodCallExpression);
                            var likeValue = ExpressionHelper.GetSqlLikeValue(methodName, propertyValue);
                            var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                            var link = ExpressionHelper.GetSqlOperator(linkingType);
                            return new QueryParameterExpression(link, propertyName, likeValue, opr, isNested);
                        }
                    default:
                        throw new NotSupportedException($"'{methodName}' method is not supported");
                }
            }

            if (expr is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType != ExpressionType.AndAlso && binaryExpression.NodeType != ExpressionType.OrElse)
                {
                    var propertyName = ExpressionHelper.GetPropertyNamePath(binaryExpression, out var isNested);

                    if (!SqlProperties.Select(x => x.PropertyName).Contains(propertyName) &&
                        !SqlJoinProperties.Select(x => x.PropertyName).Contains(propertyName))
                        throw new NotSupportedException("predicate can't parse");

                    var propertyValue = ExpressionHelper.GetValue(binaryExpression.Right);
                    var opr = ExpressionHelper.GetSqlOperator(binaryExpression.NodeType);
                    var link = ExpressionHelper.GetSqlOperator(linkingType);

                    return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                }

                var leftExpr = GetQueryProperties(binaryExpression.Left, ExpressionType.Default);
                var rightExpr = GetQueryProperties(binaryExpression.Right, binaryExpression.NodeType);

                switch (leftExpr)
                {
                    case QueryParameterExpression lQPExpr:
                        if (!string.IsNullOrEmpty(lQPExpr.LinkingOperator) && !string.IsNullOrEmpty(rightExpr.LinkingOperator)) // AND a AND B
                        {
                            switch (rightExpr)
                            {
                                case QueryBinaryExpression rQBExpr:
                                    if (lQPExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator) // AND a AND (c AND d)
                                    {
                                        var nodes = new QueryBinaryExpression
                                        {
                                            LinkingOperator = leftExpr.LinkingOperator,
                                            Nodes = new List<QueryExpression> { leftExpr }
                                        };

                                        rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                        nodes.Nodes.AddRange(rQBExpr.Nodes);

                                        leftExpr = nodes;
                                        rightExpr = null;
                                        // AND a AND (c AND d) => (AND a AND c AND d)
                                    }
                                    break;
                            }
                        }
                        break;

                    case QueryBinaryExpression lQBExpr:
                        switch (rightExpr)
                        {
                            case QueryParameterExpression rQPExpr:
                                if (rQPExpr.LinkingOperator == lQBExpr.Nodes.Last().LinkingOperator)    //(a AND b) AND c
                                {
                                    lQBExpr.Nodes.Add(rQPExpr);
                                    rightExpr = null;
                                    //(a AND b) AND c => (a AND b AND c)
                                }
                                break;

                            case QueryBinaryExpression rQBExpr:
                                if (lQBExpr.Nodes.Last().LinkingOperator == rQBExpr.LinkingOperator) // (a AND b) AND (c AND d)
                                {
                                    if (rQBExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator)   // AND (c AND d)
                                    {
                                        rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                        lQBExpr.Nodes.AddRange(rQBExpr.Nodes);
                                        // (a AND b) AND (c AND d) =>  (a AND b AND c AND d)
                                    }
                                    else
                                    {
                                        lQBExpr.Nodes.Add(rQBExpr);
                                        // (a AND b) AND (c OR d) =>  (a AND b AND (c OR d))
                                    }
                                    rightExpr = null;
                                }
                                break;
                        }
                        break;
                }

                var nLinkingOperator = ExpressionHelper.GetSqlOperator(linkingType);
                if (rightExpr == null)
                {
                    leftExpr.LinkingOperator = nLinkingOperator;
                    return leftExpr;
                }

                return new QueryBinaryExpression
                {
                    NodeType = QueryExpressionType.Binary,
                    LinkingOperator = nLinkingOperator,
                    Nodes = new List<QueryExpression> { leftExpr, rightExpr },
                };
            }

            return GetQueryProperties(ExpressionHelper.GetBinaryExpression(expr), linkingType);
        }
        
        /// <summary>
        /// Get query properties
        /// </summary>
        /// <param name="expr">The expression.</param>
        private List<QueryExpression> GetQueryProperties(Expression expr)
        {
            var queryNode = GetQueryProperties(expr, ExpressionType.Default);
            switch (queryNode)
            {
                case QueryParameterExpression qpExpr:
                    return new List<QueryExpression> { queryNode };

                case QueryBinaryExpression qbExpr:
                    return qbExpr.Nodes;

                default:
                    throw new NotSupportedException(queryNode.ToString());
            }
        }

        private string AppendJoinToSelect(SqlQuery originalBuilder, params Expression<Func<TEntity, object>>[] includes)
        {
            var joinBuilder = new StringBuilder();

            foreach (var include in includes)
            {
                var joinProperty = AllProperties.First(q => q.Name == ExpressionHelper.GetPropertyName(include));
                var declaringType = joinProperty.DeclaringType.GetTypeInfo();
                var tableAttribute = declaringType.GetCustomAttribute<TableAttribute>();
                var tableName = tableAttribute != null ? tableAttribute.Name : declaringType.Name;

                var attrJoin = joinProperty.GetCustomAttribute<JoinAttributeBase>();

                if (attrJoin == null)
                    continue;

                var joinString = "";
                if (attrJoin is LeftJoinAttribute)
                    joinString = "LEFT JOIN";
                else if (attrJoin is InnerJoinAttribute)
                    joinString = "INNER JOIN";
                else if (attrJoin is RightJoinAttribute)
                    joinString = "RIGHT JOIN";

                var joinType = joinProperty.PropertyType.IsGenericType ? joinProperty.PropertyType.GenericTypeArguments[0] : joinProperty.PropertyType;
                var properties = TypeExtensions.FindClassProperties(joinType).Where(ExpressionHelper.GetPrimitivePropertiesPredicate());
                var props = properties.Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any()).Select(p => new SqlPropertyMetadata(p)).ToArray();

                if (Config.UseQuotationMarks)
                    switch (Config.SqlProvider)
                    {
                        case SqlProvider.SQLSERVER:
                            tableName = "[" + tableName + "]";
                            attrJoin.TableName = GetTableNameWithSchemaPrefix(attrJoin.TableName, attrJoin.TableSchema, "[", "]");
                            attrJoin.Key = "[" + attrJoin.Key + "]";
                            attrJoin.ExternalKey = "[" + attrJoin.ExternalKey + "]";
                            attrJoin.TableAlias = "[" + attrJoin.TableAlias + "]";
                            foreach (var prop in props)
                                prop.ColumnName = "[" + prop.ColumnName + "]";
                            break;

                        case SqlProvider.MySQL:
                            tableName = "`" + tableName + "`";
                            attrJoin.TableName = GetTableNameWithSchemaPrefix(attrJoin.TableName, attrJoin.TableSchema, "`", "`");
                            attrJoin.Key = "`" + attrJoin.Key + "`";
                            attrJoin.ExternalKey = "`" + attrJoin.ExternalKey + "`";
                            attrJoin.TableAlias = "`" + attrJoin.TableAlias + "`";
                            foreach (var prop in props)
                                prop.ColumnName = "`" + prop.ColumnName + "`";
                            break;

                        case SqlProvider.PostgreSQL:
                            tableName = "\"" + tableName + "\"";
                            attrJoin.TableName = GetTableNameWithSchemaPrefix(attrJoin.TableName, attrJoin.TableSchema, "\"", "\"");
                            attrJoin.Key = "\"" + attrJoin.Key + "\"";
                            attrJoin.ExternalKey = "\"" + attrJoin.ExternalKey + "\"";
                            attrJoin.TableAlias = "\"" + attrJoin.TableAlias + "\"";
                            foreach (var prop in props)
                                prop.ColumnName = "\"" + prop.ColumnName + "\"";
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(Config.SqlProvider));
                    }
                else
                    attrJoin.TableName = GetTableNameWithSchemaPrefix(attrJoin.TableName, attrJoin.TableSchema);

                originalBuilder.SqlBuilder.Append($", {GetFieldsSelect(attrJoin.TableAlias, props)}");
                joinBuilder.Append(
                    $"{joinString} {attrJoin.TableName} AS {attrJoin.TableAlias} ON {tableName}.{attrJoin.Key} = {attrJoin.TableAlias}.{attrJoin.ExternalKey} ");
            }

            return joinBuilder.ToString();
        }
        
        private SqlQuery InitBuilderSelect(bool firstOnly)
        {
            var query = new SqlQuery();
            query.SqlBuilder.Append("SELECT ");
            if (firstOnly && Config.SqlProvider == SqlProvider.SQLSERVER)
                query.SqlBuilder.Append("TOP 1 ");

            query.SqlBuilder.Append(GetFieldsSelect(TableName, SqlProperties));

            return query;
        }
        
        private static string GetFieldsSelect(string tableName, SqlPropertyMetadata[] properties)
        {
            //Projection function
            string ProjectionFunction(SqlPropertyMetadata p)
            {
                return !string.IsNullOrEmpty(p.Alias)
                    ? $"{tableName}.{p.ColumnName} AS {p.PropertyName}"
                    : $"{tableName}.{p.ColumnName}";
            }

            return string.Join(", ", properties.Select(ProjectionFunction));
        }
        
        private SqlQuery InitBuilderCountWithDistinct(SqlPropertyMetadata sqlProperty)
        {
            var query = new SqlQuery();
            query.SqlBuilder.Append("SELECT COUNT(DISTINCT ");

            query.SqlBuilder
                .Append(TableName)
                .Append(".")
                .Append(sqlProperty.ColumnName)
                .Append(")");

            if (sqlProperty.Alias != null)
                query.SqlBuilder
                    .Append(" AS ")
                    .Append(sqlProperty.PropertyName);

            return query;
        }
        
        private SqlQuery GetSelect(Expression<Func<TEntity, bool>> predicate, bool firstOnly, params Expression<Func<TEntity, object>>[] includes)
        {
            var sqlQuery = InitBuilderSelect(firstOnly);

            var joinsBuilder = AppendJoinToSelect(sqlQuery, includes);
            sqlQuery.SqlBuilder
                .Append(" FROM ")
                .Append(TableName)
                .Append(" ");
            
            if (includes.Any())                  
                sqlQuery.SqlBuilder.Append(joinsBuilder);
            
            AppendWherePredicateQuery(sqlQuery, predicate, QueryType.Select);

            if (firstOnly && (Config.SqlProvider == SqlProvider.MySQL || Config.SqlProvider == SqlProvider.PostgreSQL))
                sqlQuery.SqlBuilder.Append("LIMIT 1");

            return sqlQuery;
        }
        #endregion


        #region Properties

        /// <inheritdoc />
        public PropertyInfo[] AllProperties { get; protected set; }

        /// <inheritdoc />
        public bool HasUpdatedAt => UpdatedAtProperty != null;

        /// <inheritdoc />
        public PropertyInfo UpdatedAtProperty { get; protected set; }

        /// <inheritdoc />
        public SqlPropertyMetadata UpdatedAtPropertyMetadata { get; protected set; }

        /// <inheritdoc />
        public bool IsIdentity => IdentitySqlProperty != null;

        /// <inheritdoc />
        public string TableName { get;  set; }

        /// <inheritdoc />
        public string TableSchema { get;  set; }

        /// <inheritdoc />
        public SqlPropertyMetadata IdentitySqlProperty { get; protected set; }

        /// <inheritdoc />
        public SqlPropertyMetadata[] KeySqlProperties { get; protected set; }

        /// <inheritdoc />
        public SqlPropertyMetadata[] SqlProperties { get; protected set; }

        /// <inheritdoc />
        public SqlJoinPropertyMetadata[] SqlJoinProperties { get; protected set; }

        /// <inheritdoc />
        public SqlAdapterConfig Config { get; protected set; }

        /// <inheritdoc />
        public bool LogicalDelete { get; protected set; }

        /// <inheritdoc />
        public string StatusPropertyName { get; protected set; }

        /// <inheritdoc />
        public object LogicalDeleteValue { get; protected set; }
        

        #endregion
        
        
        
        public virtual SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate)
        {
            var sqlQuery = new SqlQuery();

            sqlQuery.SqlBuilder
                .Append("SELECT COUNT(*) FROM ")
                .Append(TableName)
                .Append(" ");

            AppendWherePredicateQuery(sqlQuery, predicate, QueryType.Select);

            return sqlQuery;
        }

        public virtual SqlQuery GetCount(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var propertyName = ExpressionHelper.GetPropertyName(distinctField);
            var property = SqlProperties.First(x => x.PropertyName == propertyName);
            var sqlQuery = InitBuilderCountWithDistinct(property);

            sqlQuery.SqlBuilder
                .Append(" FROM ")
                .Append(TableName)
                .Append(" ");

            AppendWherePredicateQuery(sqlQuery, predicate, QueryType.Select);

            return sqlQuery;
        }

        public virtual SqlQuery GetInsert(TEntity entity)
        {
            var properties =
                (IsIdentity
                    ? SqlProperties.Where(p => !p.PropertyName.Equals(IdentitySqlProperty.PropertyName, StringComparison.OrdinalIgnoreCase))
                    : SqlProperties).ToList();

            if (HasUpdatedAt)
                UpdatedAtProperty.SetValue(entity, DateTime.UtcNow);

            var query = new SqlQuery(entity);

            query.SqlBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", TableName, string.Join(", ", properties.Select(p => p.ColumnName)),
                string.Join(", ", properties.Select(p => "@" + p.PropertyName))); // values

            return query;
        }

        public virtual SqlQuery GetBulkInsert(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");

            var entityType = entitiesArray[0].GetType();

            var properties =
                (IsIdentity
                    ? SqlProperties.Where(p => !p.PropertyName.Equals(IdentitySqlProperty.PropertyName, StringComparison.OrdinalIgnoreCase))
                    : SqlProperties).ToList();

            var query = new SqlQuery();

            var values = new List<string>();
            var parameters = new Dictionary<string, object>();

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                if (HasUpdatedAt)
                    UpdatedAtProperty.SetValue(entitiesArray[i], DateTime.UtcNow);

                foreach (var property in properties)
                    // ReSharper disable once PossibleNullReferenceException
                    parameters.Add(property.PropertyName + i, entityType.GetProperty(property.PropertyName).GetValue(entitiesArray[i], null));

                values.Add($"({string.Join(", ", properties.Select(p => "@" + p.PropertyName + i))})");
            }

            query.SqlBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES {2}", TableName, string.Join(", ", properties.Select(p => p.ColumnName)), string.Join(",", values)); // values

            query.SetParam(parameters);

            return query;
        }

        public virtual SqlQuery GetUpdate(TEntity entity)
        {
            var properties = SqlProperties.Where(p =>
                !KeySqlProperties.Any(k => k.PropertyName.Equals(p.PropertyName, StringComparison.OrdinalIgnoreCase)) && !p.IgnoreUpdate).ToArray();
            if (!properties.Any())
                throw new ArgumentException("Can't update without [Key]");

            if (HasUpdatedAt)
                UpdatedAtProperty.SetValue(entity, DateTime.UtcNow);

            var query = new SqlQuery(entity);

            query.SqlBuilder
                .Append("UPDATE ")
                .Append(TableName)
                .Append(" SET ");

            query.SqlBuilder.Append(string.Join(", ", properties
                .Select(p => $"{p.ColumnName} = @{p.PropertyName}")));

            query.SqlBuilder.Append(" WHERE ");

            query.SqlBuilder.Append(string.Join(" AND ", KeySqlProperties.Where(p => !p.IgnoreUpdate)
                .Select(p => $"{p.ColumnName} = @{p.PropertyName}")));

            return query;
        }

        public virtual SqlQuery GetUpdate(Expression<Func<TEntity, bool>> predicate, TEntity entity)
        {
            var properties = SqlProperties.Where(p =>
                !KeySqlProperties.Any(k => k.PropertyName.Equals(p.PropertyName, StringComparison.OrdinalIgnoreCase)) && !p.IgnoreUpdate).ToArray();

            if (HasUpdatedAt)
                UpdatedAtProperty.SetValue(entity, DateTime.UtcNow);

            var query = new SqlQuery(entity);

            query.SqlBuilder
                .Append("UPDATE ")
                .Append(TableName)
                .Append(" SET ");

            query.SqlBuilder.Append(string.Join(", ", properties
                .Select(p => $"{p.ColumnName} = @{p.PropertyName}")));

            query.SqlBuilder
                .Append(" ");
            
            AppendWherePredicateQuery(query, predicate, QueryType.Update);

            var parameters = new Dictionary<string, object>();
            var entityType = entity.GetType();
            foreach (var property in properties)
                parameters.Add(property.PropertyName, entityType.GetProperty(property.PropertyName).GetValue(entity, null));

            if (query.Param is Dictionary<string, object> whereParam)
                parameters.AddRange(whereParam);

            query.SetParam(parameters);

            return query;
        }

        public virtual SqlQuery GetBulkUpdate(IEnumerable<TEntity> entities)
        {
            var entitiesArray = entities as TEntity[] ?? entities.ToArray();
            if (!entitiesArray.Any())
                throw new ArgumentException("collection is empty");

            var entityType = entitiesArray[0].GetType();

            var properties = SqlProperties.Where(p =>
                !KeySqlProperties.Any(k => k.PropertyName.Equals(p.PropertyName, StringComparison.OrdinalIgnoreCase)) && !p.IgnoreUpdate).ToArray();

            var query = new SqlQuery();

            var parameters = new Dictionary<string, object>();

            for (var i = 0; i < entitiesArray.Length; i++)
            {
                if (HasUpdatedAt)
                    UpdatedAtProperty.SetValue(entitiesArray[i], DateTime.UtcNow);

                if (i > 0)
                    query.SqlBuilder.Append("; ");

                query.SqlBuilder.Append(
                    $"UPDATE {TableName} SET {string.Join(", ", properties.Select(p => $"{p.ColumnName} = @{p.PropertyName}{i}"))} WHERE {string.Join(" AND ", KeySqlProperties.Where(p => !p.IgnoreUpdate).Select(p => $"{p.ColumnName} = @{p.PropertyName}{i}"))}");

                // ReSharper disable PossibleNullReferenceException
                foreach (var property in properties)
                    parameters.Add(property.PropertyName + i, entityType.GetProperty(property.PropertyName).GetValue(entitiesArray[i], null));

                foreach (var property in KeySqlProperties.Where(p => !p.IgnoreUpdate))
                    parameters.Add(property.PropertyName + i, entityType.GetProperty(property.PropertyName).GetValue(entitiesArray[i], null));

                // ReSharper restore PossibleNullReferenceException
            }

            query.SetParam(parameters);

            return query;
        }

        public virtual SqlQuery GetSelectById(object id, params Expression<Func<TEntity, object>>[] includes)
        {
            if (KeySqlProperties.Length != 1)
                throw new NotSupportedException("GetSelectById support only 1 key");

            var keyProperty = KeySqlProperties[0];

            var sqlQuery = InitBuilderSelect(true);

            if (includes.Any())
            {
                var joinsBuilder = AppendJoinToSelect(sqlQuery, includes);
                sqlQuery.SqlBuilder
                    .Append(" FROM ")
                    .Append(TableName)
                    .Append(" ");

                sqlQuery.SqlBuilder.Append(joinsBuilder);
            }
            else
            {
                sqlQuery.SqlBuilder
                    .Append(" FROM ")
                    .Append(TableName)
                    .Append(" ");
            }

            IDictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { keyProperty.PropertyName, id }
            };

            sqlQuery.SqlBuilder
                .Append("WHERE ")
                .Append(TableName)
                .Append(".")
                .Append(keyProperty.ColumnName)
                .Append(" = @")
                .Append(keyProperty.PropertyName)
                .Append(" ");

            if (LogicalDelete)
                sqlQuery.SqlBuilder
                    .Append("AND ")
                    .Append(TableName)
                    .Append(".")
                    .Append(StatusPropertyName)
                    .Append(" != ")
                    .Append(LogicalDeleteValue)
                    .Append(" ");

            sqlQuery.SetParam(dictionary);
            return sqlQuery;
        }

        public virtual SqlQuery GetSelectFirst(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            return GetSelect(predicate, true, includes);
        }

        public virtual SqlQuery GetSelectAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            return GetSelect(predicate, false, includes);
        }

        public virtual SqlQuery GetSelectBetween(object @from, object to, Expression<Func<TEntity, object>> btwField)
        {
            return GetSelectBetween(from, to, btwField, null);
        }

        public virtual SqlQuery GetSelectBetween(object @from, object to, Expression<Func<TEntity, object>> btwField, Expression<Func<TEntity, bool>> predicate)
        {
            var fieldName = ExpressionHelper.GetPropertyName(btwField);
            var columnName = SqlProperties.First(x => x.PropertyName == fieldName).ColumnName;
            var query = GetSelectAll(predicate);

            query.SqlBuilder
                .Append(predicate == null && !LogicalDelete ? "WHERE" : "AND")
                .Append(" ")
                .Append(TableName)
                .Append(".")
                .Append(columnName)
                .Append(" BETWEEN '")
                .Append(from)
                .Append("' AND '")
                .Append(to)
                .Append("'");

            return query;
        }

        public virtual SqlQuery GetDelete(TEntity entity)
        {
            var sqlQuery = new SqlQuery();
            var whereAndSql = 
                string.Join(" AND ", KeySqlProperties.Select(p => $"{TableName}.{p.ColumnName} = @{p.PropertyName}"));

            if (!LogicalDelete)
            {
                sqlQuery.SqlBuilder
                    .Append("DELETE FROM ")
                    .Append(TableName)
                    .Append(" WHERE ")
                    .Append(whereAndSql);
            }
            else
            {                
                sqlQuery.SqlBuilder
                    .Append("UPDATE ")
                    .Append(TableName)
                    .Append(" SET ")
                    .Append(StatusPropertyName)
                    .Append(" = ")
                    .Append(LogicalDeleteValue);

                if (HasUpdatedAt)
                {
                    UpdatedAtProperty.SetValue(entity, DateTime.UtcNow);

                    sqlQuery.SqlBuilder
                        .Append(", ")
                        .Append(UpdatedAtPropertyMetadata.ColumnName)
                        .Append(" = @")
                        .Append(UpdatedAtPropertyMetadata.PropertyName);
                }

                sqlQuery.SqlBuilder 
                    .Append(" WHERE ")
                    .Append(whereAndSql);
            }

            sqlQuery.SetParam(entity);
            return sqlQuery;
        }

        public virtual SqlQuery GetDelete(Expression<Func<TEntity, bool>> predicate)
        {
            var sqlQuery = new SqlQuery();

            if (!LogicalDelete)
            {
                sqlQuery.SqlBuilder
                    .Append("DELETE FROM ")
                    .Append(TableName);
            }
            else
            {
                sqlQuery.SqlBuilder
                    .Append("UPDATE ")
                    .Append(TableName)
                    .Append(" SET ")
                    .Append(StatusPropertyName)
                    .Append(" = ")
                    .Append(LogicalDeleteValue);

                if (HasUpdatedAt)
                    sqlQuery.SqlBuilder
                        .Append(", ")
                        .Append(UpdatedAtPropertyMetadata.ColumnName)
                        .Append(" = @")
                        .Append(UpdatedAtPropertyMetadata.PropertyName);

               
            }
            sqlQuery.SqlBuilder.Append(" ");
            AppendWherePredicateQuery(sqlQuery, predicate, QueryType.Delete);
            return sqlQuery;
        }
    }
}