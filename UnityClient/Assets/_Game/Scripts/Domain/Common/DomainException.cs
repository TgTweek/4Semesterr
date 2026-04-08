using System;

namespace Game.Domain.Common
{
    public sealed class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {

        }
    }
}
