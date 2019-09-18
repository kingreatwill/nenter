using System;
using System.Data;

namespace Nenter.Data
{
    public interface IDbContext : IDisposable
    {
        /// <summary>
        ///     Get opened DB Connection
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        ///     Open DB connection
        /// </summary>
        void OpenConnection();

        /// <summary>
        ///     Open DB connection and Begin transaction
        /// </summary>
        IDbTransaction BeginTransaction();
    }
}