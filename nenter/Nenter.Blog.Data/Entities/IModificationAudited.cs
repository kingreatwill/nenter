using System;

namespace Nenter.Blog.Data.Entities
{
    public interface IModificationAudited
    {
        DateTime? LastModificationTime { get; set; }
        long? LastModifierUserId { get; set; }
    }
}