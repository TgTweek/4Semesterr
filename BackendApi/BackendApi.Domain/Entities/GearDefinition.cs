using System;
using System.Collections.Generic;

namespace BackendApi.Domain.Entities
{
    public sealed class GearDefinition
    {
        public Guid GearDefinitionId { get; set; }

        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Allowed values for now:
        // Helmet, Chest, Legs
        public string Slot { get; set; } = string.Empty;

        // Allowed values for now:
        // Common, Uncommon, Rare
        public string Rarity { get; set; } = string.Empty;

        public int Price { get; set; }
        public int ArmorValue { get; set; }

       
        public string? SetKey { get; set; }

        public string IconKey { get; set; } = string.Empty;

        public bool IsMerchantAvailable { get; set; } = true;
    }
}