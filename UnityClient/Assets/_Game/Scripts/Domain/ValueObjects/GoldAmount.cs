using Game.Domain.Common;

namespace Game.Domain.ValueObjects
{
    public sealed class GoldAmount
    {
        public int Value { get; }

        public GoldAmount(int value)
        {
            if (value < 0)
                throw new DomainException("Gold cannot be negative.");

            Value = value;
        }

        public bool CanAfford(GoldAmount price)
        {
            return Value >= price.Value;
        }

        public GoldAmount Add(GoldAmount other)
        {
            return new GoldAmount(Value + other.Value);
        }

        public GoldAmount Subtract(GoldAmount other)
        {
            if (!CanAfford(other))
                throw new DomainException("Not enough gold.");

            return new GoldAmount(Value - other.Value);
        }
    }
}
