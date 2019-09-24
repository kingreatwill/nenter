namespace Nenter.Data.Sql
{
    public class ISqlBuilder
    {
        public ISqlBuilder()
        {
            
        }
    }
    
    public class IWhereBuilder
    {
        public IWhereBuilder()
        {
            
        }
        
        // AND OR
    }

    public static class SqlBuilderExtensions
    {
        public static ISqlBuilder AddWhereRequest<T>(this ISqlBuilder builder,T request)
        {
            return builder;
        }
        
        public static ISqlBuilder AddSelectResponse<T>(this ISqlBuilder builder,T response)
        {
            return builder;
        }
    }
}