using Game.Domain.Common;

namespace Game.Domain.ValueObjects
{
    public sealed class OfferId
    {
        public string Value { get; }

        public OfferId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("OfferId cannot be empty.");

            Value = value;
        }
    }
}