 namespace Nenter.Dapper.Linq.Helpers
{
    public class SqlServerWriter<TData> : SqlWriter<TData>
    {
        public SqlServerWriter():base("[","]")
        {
        }
    
    }
}
