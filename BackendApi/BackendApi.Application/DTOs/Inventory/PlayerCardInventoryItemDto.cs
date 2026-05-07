namespace BackendApi.Application.DTOs.Inventory
{
    public sealed class PlayerCardInventoryItemDto
    {
        public Guid PlayerCardId { get; set; }
        public Guid CardDefinitionId { get; set; }

        public string CardKey { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int ManaCost { get; set; }
        public string EffectType { get; set; } = string.Empty;
        public int EffectValue { get; set; }

        public string Rarity { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public int LoadoutOrder { get; set; }
    }
}