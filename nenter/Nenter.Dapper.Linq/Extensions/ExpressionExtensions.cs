using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Nenter.Core.Extensions;
using Nenter.Dapper.Linq.Helpers;

namespace Nenter.Dapper.Linq.Extensions
{
    public static class ExpressionExtensions
    {
       internal static bool IsEqualsExpression(this Expression exp)
        {
            return exp.NodeType == ExpressionType.Equal || exp.NodeType == ExpressionType.NotEqual;
        }

        internal static bool IsSpecificMemberExpression(this Expression exp, Type declaringType, string memberName)
        {
            return ((exp is MemberExpression) &&
                (((MemberExpression)exp).Member.DeclaringType == declaringType) &&
                (((MemberExpression)exp).Member.Name == memberName));
        }

        internal static bool IsSpecificMemberExpression(this Expression exp, Type declaringType, SortedDictionary<string, EntityColumn> propertyList)
        {
            if (propertyList == null) return false;
            return ((exp is MemberExpression) &&
                    (((MemberExpression)exp).Member.DeclaringType == declaringType) &&
                    propertyList[(((MemberExpression)exp).Member.Name)] != null);
        }

        internal static object GetValueFromEqualsExpression(this BinaryExpression be, Type memberDeclaringType)
        {
            
            if (!be.IsEqualsExpression())
                throw new Exception("There is a bug in this program.");

            if (be.Left.NodeType == ExpressionType.MemberAccess)
            {
                var me = (MemberExpression)be.Left;

                if (me.Member.DeclaringType == memberDeclaringType)
                {
                    return GetValueFromExpression(be.Right);
                }
            }
            else if (be.Right.NodeType == ExpressionType.MemberAccess)
            {
                var me = (MemberExpression)be.Right;

                if (me.Member.DeclaringType == memberDeclaringType)
                {
                    return GetValueFromExpression(be.Left);
                }
            }

            // We should have returned by now. 
            throw new Exception("There is a bug in this program.");
        }

        internal static string GetPropertyNameFromEqualsExpression(this BinaryExpression be, Type memberDeclaringType)
        {
            if (!be.IsEqualsExpression())
                throw new Exception("There is a bug in this program.");

            if (be.Left.NodeType == ExpressionType.MemberAccess)
            {
                return be.Left.GetPropertyNameFromExpression();
            }
            if (be.Right.NodeType == ExpressionType.MemberAccess)
            {
                return be.Right.GetPropertyNameFromExpression();
            }

            // We should have returned by now. 
            throw new Exception("There is a bug in this program.");
        }

        internal static string GetIndentifierFromExpression(this Expression expression)
        {
            return expression.GetTableFromExpression().Identifier;
        }

        internal static EntityTable GetTableFromExpression(this Expression expression)
        {
            var exp = expression.GetMemberExpression();
            if (!(exp is MemberExpression)) return null;

            return EntityTableCacheHelper.TryGetTable(((MemberExpression)exp).Expression.Type);
        }
      

        internal static string GetPropertyNameFromExpression(this Expression expression)
        {
            var exp = expression.GetMemberExpression();
            if (!(exp is MemberExpression)) return string.Empty;

            var member = ((MemberExpression)exp).Member;
            var columns = EntityTableCacheHelper.TryGetPropertyList(((MemberExpression)exp).Expression.Type);
            return columns[member.Name].ColumnName;
        }

        internal static Expression GetMemberExpression(this Expression expression)
        {
            if (expression is UnaryExpression)
                return (((UnaryExpression)expression).Operand).GetMemberExpression();
            if (expression is LambdaExpression)
                return (((LambdaExpression)expression).Body).GetMemberExpression();
            if (expression is MemberExpression)
                return expression;
            return null;
        }

        internal static object GetValueFromExpression(this Expression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        internal static string GetOperator(string methodName)
        {
            switch (methodName)
            {
                case "Add": return "+";
                case "Subtract": return "-";
                case "Multiply": return "*";
                case "Divide": return "/";
                case "Negate": return "-";
                case "Remainder": return "%";
                default: return null;
            }
        }

        internal static string GetOperator(this UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return "-";
                case ExpressionType.UnaryPlus:
                    return "+";
                case ExpressionType.Not:
                    return u.Operand.Type.IsBoolean() ? "NOT" : "~";
                default:
                    return "";
            }
        }

        internal static string GetOperator(this BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return b.Left.Type.IsBoolean() ? "AND" : "&";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return b.Left.Type.IsBoolean() ? "OR" : "|";
                default:
                    return GetOperator(b.NodeType);
            }
        }

        internal static string GetOperator(this ExpressionType exprType)
        {
            switch (exprType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                default:
                    return "";
            }
        }

        internal static bool IsHasValue(this Expression expr)
        {
            return (expr is MemberExpression) && (((MemberExpression)expr).Member.Name == "HasValue");
        }

        internal static bool IsPredicate(this Expression expr)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Not:
                case ExpressionType.Call:
                    return expr.Type.IsBoolean();
                
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                
                default:
                    return false;
            }
        }

        internal static bool IsVariable(this Expression expr)
        {
            return (expr is MemberExpression) && (((MemberExpression)expr).Expression is ConstantExpression);
        }
       
    }
}