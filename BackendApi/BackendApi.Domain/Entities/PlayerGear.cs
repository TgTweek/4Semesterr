using System;

namespace BackendApi.Domain.Entities
{
    public sealed class PlayerGear
    {
        public Guid PlayerGearId { get; set; }

        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public Guid GearDefinitionId { get; set; }
        public GearDefinition GearDefinition { get; set; } = null!;

        public string Location { get; set; } = InventoryItemLocation.Stash;

        public DateTime AcquiredAtUtc { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}