using Game.Domain.Common;

namespace Game.Domain.ValueObjects
{
    public sealed class MerchantId
    {
        public string Value { get; }

        public MerchantId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("MerchantId cannot be empty.");

            Value = value;
        }
    }
}