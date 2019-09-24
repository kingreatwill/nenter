using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Nenter.Data.EntityFramework
{
    public class EFDbContext : DbContext,IDbContext
    {
        private readonly IDbConnection _innerConnection;

        public EFDbContext(IDbConnection connection)
        {
            _innerConnection = connection;
        }
        
        public virtual IDbConnection Connection
        { 
            get
            {
                OpenConnection();
                return _innerConnection;
            } 
        }
        
        public virtual void OpenConnection()
        {
            if (_innerConnection.State != ConnectionState.Open && _innerConnection.State != ConnectionState.Connecting)
                _innerConnection.Open();
        }

        public virtual IDbTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }
        
        public override void Dispose()
        {
            if (_innerConnection != null && _innerConnection.State != ConnectionState.Closed)
                _innerConnection.Close();
        }
    }
}