﻿namespace Nenter.Data.Entities
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}