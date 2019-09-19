﻿using System;

namespace Nenter.Dapper.Linq.Exceptions
{
    public class InvalidCacheItemException : Exception
    {
        public InvalidCacheItemException(string message) : base(message)
        {
        }
    }
}
