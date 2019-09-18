﻿using System.Data;

namespace Nenter.Data.Dapper
{
    public class DbContext : IDbContext
    {
        /// <summary>
        ///     DB Connection for internal use
        /// </summary>
        protected readonly IDbConnection InnerConnection;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected DbContext(IDbConnection connection)
        {
            InnerConnection = connection;
        }

        /// <inheritdoc />
        public virtual IDbConnection Connection
        {
            get
            {
                OpenConnection();
                return InnerConnection;
            }
        }

        /// <inheritdoc />
        public void OpenConnection()
        {
            if (InnerConnection.State != ConnectionState.Open && InnerConnection.State != ConnectionState.Connecting)
                InnerConnection.Open();
        }

        /// <inheritdoc />
        public virtual IDbTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Close DB connection
        /// </summary>
        public void Dispose()
        {
            if (InnerConnection != null && InnerConnection.State != ConnectionState.Closed)
                InnerConnection.Close();
        }
    }
}