﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Nenter.Dapper.Linq
{
    public class Linq2Dapper<TData> : IOrderedQueryable<TData>
    {
        #region Constructors

        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="expression"></param>
        public Linq2Dapper(IQueryProvider provider, Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            
            if (!typeof(IQueryable<TData>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }
            
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="provider"></param>
        public Linq2Dapper(IDbConnection connection, IQueryProvider provider = null)
        {
            Connection = connection;
            Provider = provider ?? new QueryProvider<TData>(connection);
            //Expression = (Expression) expression ?? Expression.Constant(this);
            Expression = Expression.Constant(this);
        }

        #endregion

        #region Properties

        public IQueryProvider Provider { get; private set; }
        public IDbConnection Connection { get; private set; }
        public Expression Expression { get; private set; }

        public Type ElementType => typeof(TData);

        #endregion

        #region Enumerators
        public IEnumerator<TData> GetEnumerator()
        {
            return (Provider.Execute<IEnumerable<TData>>(Expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }
        #endregion
    }
}
