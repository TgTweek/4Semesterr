using System.Collections.Generic;
using Game.Domain.ValueObjects;

namespace Game.Domain.Entities
{
    public sealed class Player
    {
        private readonly List<CardDefinitionId> _ownedCards = new();

        public PlayerId Id { get; }
        public GoldAmount Gold { get; private set; }

        public IReadOnlyCollection<CardDefinitionId> OwnedCards => _ownedCards.AsReadOnly();

        public Player(PlayerId id, GoldAmount gold, IEnumerable<CardDefinitionId>? ownedCards = null)
        {
            Id = id;
            Gold = gold;

            if (ownedCards is not null)
            {
                _ownedCards.AddRange(ownedCards);
            }
        }

        public void SpendGold(GoldAmount amount)
        {
            Gold = Gold.Subtract(amount);
        }

        public void AddCard(CardDefinitionId cardDefinitionId)
        {
            _ownedCards.Add(cardDefinitionId);
        }
    }
}