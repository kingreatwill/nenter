namespace Nenter.Blog.Data.Entities
{
    public interface IPassivable
    {
        bool IsActive { get; set; }
    }
}