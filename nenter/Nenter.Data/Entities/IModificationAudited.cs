using System;

namespace Nenter.Data.Entities
{
    public interface IModificationAudited
    {
        DateTime? LastModificationTime { get; set; }
        long? LastModifierUserId { get; set; }
    }
}