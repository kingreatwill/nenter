using System;

namespace Nenter.Blog.Data.Entities
{
    public interface ICreationAudited
    {
        DateTime CreationTime { get; set; }
        
        long? CreatorUserId { get; set; }
    }
}