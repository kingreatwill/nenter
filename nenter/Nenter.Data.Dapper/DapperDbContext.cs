using System.Data;

namespace Nenter.Data.Dapper
{
    public class DapperDbContext : IDbContext
    {
        private readonly IDbConnection _innerConnection;

        public DapperDbContext(IDbConnection connection)
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
        
        public void Dispose()
        {
            if (_innerConnection != null && _innerConnection.State != ConnectionState.Closed)
                _innerConnection.Close();
        }
    }
}