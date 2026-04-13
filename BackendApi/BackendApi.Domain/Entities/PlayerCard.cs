using System;

namespace BackendApi.Domain.Entities
{
    public sealed class PlayerCard
    {
        public Guid PlayerCardId { get; set; }

        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public Guid CardDefinitionId { get; set; }
        public CardDefinition CardDefinition { get; set; } = null!;

        public DateTime AcquiredAtUtc { get; set; }
    }
}