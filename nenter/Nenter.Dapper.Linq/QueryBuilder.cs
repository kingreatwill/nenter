﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Nenter.Dapper.Linq.Exceptions;
using Nenter.Dapper.Linq.Helpers;
using Dapper;
namespace Nenter.Dapper.Linq
{
    internal class QueryBuilder<TData> : ExpressionVisitor
    {
        #region Fields
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        private ISqlWriter<TData> _serverWriter;

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        internal DynamicParameters Parameters => _serverWriter.Parameters;
        internal string Sql => _serverWriter.Sql;

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        #region ctor
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        public QueryBuilder(ISqlWriter<TData> serverWriter)
        {
             _serverWriter = serverWriter;
        }

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        #region Visitors
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        public void Evaluate(Expression node)
        {
            base.Visit(node);
        }


        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
                _serverWriter.NotOperater = true;

            //if ((node.Operand is LambdaExpression) && (((LambdaExpression)node.Operand).Body is MemberExpression))
            //    base.Visit(node.Operand);

            if (!(node.Operand is MemberExpression))
                return base.VisitUnary(node);

            Visit(node.Operand);

            if (QueryHelper.IsBoolean(node.Operand.Type) && !QueryHelper.IsHasValue(node.Operand))
                _serverWriter.Boolean(!QueryHelper.IsPredicate(node));
                
            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (QueryHelper.IsSpecificMemberExpression(node, node.Expression.Type, CacheHelper.TryGetPropertyList(node.Expression.Type)))
            {
                _serverWriter.ColumnName(QueryHelper.GetPropertyNameWithIdentifierFromExpression(node));
                return node;
            }
            else if (QueryHelper.IsVariable(node))
            {
                _serverWriter.Parameter(QueryHelper.GetValueFromExpression(node));
                return node;
            }
            else if (QueryHelper.IsHasValue(node))
            {
                var me = base.VisitMember(node);
                _serverWriter.IsNull();
                return me;
            }
            return base.VisitMember(node); ;
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ConstantExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(Linq2Dapper<TData>)) return node;
            var value = node.Value as ConstantExpression;
            var val = QueryHelper.GetValueFromExpression(value ?? node);

            _serverWriter.Parameter(val);

            return base.VisitConstant(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var op = QueryHelper.GetOperator(node);
            Expression left = node.Left;
            Expression right = node.Right;

            _serverWriter.OpenBrace();

            if (QueryHelper.IsBoolean(left.Type))
            {
                Visit(left);
                _serverWriter.WhiteSpace();
                _serverWriter.Write(op);
                _serverWriter.WhiteSpace();
                Visit(right);
            }
            else
            {
                VisitValue(left);
                _serverWriter.WhiteSpace();
                _serverWriter.Write(op);
                _serverWriter.WhiteSpace();
                VisitValue(right);
            }

            _serverWriter.CloseBrace();

            return node;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case MethodCall.EndsWith:
                case MethodCall.StartsWith:
                case MethodCall.Contains:
                    // LIKE '(%)xyz(%)' 
                    // LIKE IN (x, y, s)
                    return LikeInMethod(node);
                case MethodCall.IsNullOrEmpty:
                    // ISNULL(x, '') (!)= ''
                    if (IsNullMethod(node)) return node;
                    break;
                case MethodCall.Join:
                    return JoinMethod(node);
                case MethodCall.Take:
                    // TOP(..)
                    _serverWriter.TopCount = (int)QueryHelper.GetValueFromExpression(node.Arguments[1]);
                    return node;
                case MethodCall.Single:
                case MethodCall.First:
                case MethodCall.FirstOrDefault:
                    // TOP(1)
                    _serverWriter.TopCount = 1;
                    return Visit(node.Arguments[node.Arguments.Count-1]);
                case MethodCall.Distinct:
                    // DISTINCT
                    _serverWriter.IsDistinct = true;
                    return node;
                case MethodCall.OrderBy:
                case MethodCall.ThenBy:
                case MethodCall.OrderByDescending:
                case MethodCall.ThenByDescending:
                    // ORDER BY ...
                    _serverWriter.WriteOrder(QueryHelper.GetPropertyNameWithIdentifierFromExpression(node.Arguments[1]), node.Method.Name.Contains("Descending"));
                    return Visit(node.Arguments[0]);
                case MethodCall.Select:
                    var type = ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Body.Type;
                    QueryHelper.GetTypeProperties(type);
                    _serverWriter.SelectType = type;
                    return base.VisitMethodCall(node);
            }
            return base.VisitMethodCall(node);
        }

        protected virtual Expression VisitValue(Expression expr)
        {
            return Visit(expr);
        }

        protected virtual Expression VisitPredicate(Expression expr)
        {
            if (!QueryHelper.IsPredicate(expr) && !QueryHelper.IsHasValue(expr))
            {
                _serverWriter.Boolean(true);
            }
            return expr;
        }

        protected virtual Expression VisitQuote(Expression expr)
        {
            return expr;
        }

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------------------------

        #region Private methods
        //--------------------------------------------------------------------------------------------------------------------------------------------------
        private Expression JoinMethod(MethodCallExpression expression)
        {
            // first argument is another join or method call
            if (expression.Arguments[0] is MethodCallExpression) VisitMethodCall((MethodCallExpression)expression.Arguments[0]);

            var joinFromType = ((LambdaExpression)((UnaryExpression)expression.Arguments[4]).Operand).Parameters[0].Type;

            // from type if generic, possbily another join
            if (joinFromType.IsGenericType) joinFromType = joinFromType.GenericTypeArguments[1];
            var joinToType = ((LambdaExpression)((UnaryExpression)expression.Arguments[4]).Operand).Parameters[1].Type;

            QueryHelper.GetTypeProperties(joinFromType);
            var joinToTable = QueryHelper.GetTypeProperties(joinToType);

            var primaryJoinColumn = QueryHelper.GetPropertyNameWithIdentifierFromExpression(expression.Arguments[2]);
            var secondaryJoinColumn = QueryHelper.GetPropertyNameWithIdentifierFromExpression(expression.Arguments[3]);

            _serverWriter.WriteJoin(joinToTable.Name, joinToTable.Identifier, primaryJoinColumn, secondaryJoinColumn);

            return expression;
        }


        private bool IsNullMethod(MethodCallExpression node)
        {
            if (!QueryHelper.IsSpecificMemberExpression(node.Arguments[0], typeof (TData),
                    CacheHelper.TryGetPropertyList<TData>())) return false;

            _serverWriter.IsNullFunction();
            _serverWriter.OpenBrace();
            Visit(node.Arguments[0]);
            _serverWriter.Delimiter();
            _serverWriter.WhiteSpace();
            _serverWriter.EmptyString();
            _serverWriter.CloseBrace();
            _serverWriter.WhiteSpace();
            _serverWriter.Operator();
            _serverWriter.WhiteSpace();
            _serverWriter.EmptyString();
            return true;
        }

        private Expression LikeInMethod(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                // LIKE '..'
                if (!QueryHelper.IsSpecificMemberExpression(node.Object, typeof(TData), CacheHelper.TryGetPropertyList<TData>()))
                    return node;

                Visit(node.Object);
                _serverWriter.Like();
                if (node.Method.Name == MethodCall.EndsWith || node.Method.Name == MethodCall.Contains) _serverWriter.LikePrefix();
                Visit(node.Arguments[0]);
                if (node.Method.Name == MethodCall.StartsWith || node.Method.Name == MethodCall.Contains) _serverWriter.LikeSuffix();
                return node;
            }

            // IN (...)
            object ev;

            if (node.Method.DeclaringType == typeof (List<string>))
            {
                if (
                    !QueryHelper.IsSpecificMemberExpression(node.Arguments[0], typeof (TData),
                        CacheHelper.TryGetPropertyList<TData>()))
                    return node;


                Visit(node.Arguments[0]);
                ev = QueryHelper.GetValueFromExpression(node.Object);

            }
            else if (node.Method.DeclaringType == typeof (Enumerable))
            {
                if (
                    !QueryHelper.IsSpecificMemberExpression(node.Arguments[1], typeof (TData),
                        CacheHelper.TryGetPropertyList<TData>()))
                    return node;

                Visit(node.Arguments[1]);
                ev = QueryHelper.GetValueFromExpression(node.Arguments[0]);

            }
            else
            {
                return node;
            }
            
            _serverWriter.In();

            // Add each string in the collection to the list of locations to obtain data about. 
            var queryStrings = (IList<object>)ev;
            var count = queryStrings.Count();
            _serverWriter.OpenBrace();
            for (var i = 0; i < count; i++)
            {
                _serverWriter.Parameter(queryStrings.ElementAt(i));

                if (i + 1 < count)
                    _serverWriter.Delimiter();
            }
            _serverWriter.CloseBrace();

            return node;
        }

        #endregion
        //--------------------------------------------------------------------------------------------------------------------------------------------------
    }
}