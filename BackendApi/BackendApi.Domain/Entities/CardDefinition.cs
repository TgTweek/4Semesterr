using System;

namespace BackendApi.Domain.Entities
{
    public sealed class CardDefinition
    {
        public Guid CardDefinitionId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ManaCost { get; set; }
        public int Price { get; set; }
        public string EffectType { get; set; } = string.Empty;
        public int EffectValue { get; set; }
        public string Rarity { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsMerchantAvailable { get; set; } = true;
    }
}