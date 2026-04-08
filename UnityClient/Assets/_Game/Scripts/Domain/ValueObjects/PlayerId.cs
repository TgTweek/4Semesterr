using Game.Domain.Common;

namespace Game.Domain.ValueObjects
{
    public sealed class PlayerId
    {
        public string Value { get; }

        public PlayerId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("PlayerId cannot be empty.");

            Value = value;
        }
    }
}