using System;

namespace Nenter.Data.Entities
{
    public interface ICreationAudited
    {
        DateTime CreationTime { get; set; }
        
        long? CreatorUserId { get; set; }
    }
}