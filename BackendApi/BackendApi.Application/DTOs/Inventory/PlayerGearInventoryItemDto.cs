namespace BackendApi.Application.DTOs.Inventory
{
    public sealed class PlayerGearInventoryItemDto
    {
        public Guid PlayerGearId { get; set; }
        public Guid GearDefinitionId { get; set; }

        public string GearKey { get; set; } = string.Empty;
        public string GearName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Slot { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;

        public int ArmorValue { get; set; }

        public string SetKey { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public string ThreePieceBonusDescription { get; set; } = string.Empty;

        public string IconKey { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;
    }
}