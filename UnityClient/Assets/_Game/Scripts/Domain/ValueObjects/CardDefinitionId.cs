using Game.Domain.Common;

namespace Game.Domain.ValueObjects
{
    public sealed class CardDefinitionId
    {
        public string Value { get; }

        public CardDefinitionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("CardDefinitionId cannot be empty.");

            Value = value;
        }
    }
}