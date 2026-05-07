namespace BackendApi.Application.DTOs.Inventory
{
    public sealed class PlayerInventoryResponseDto
    {
        public List<PlayerCardInventoryItemDto> Cards { get; set; } = new();
        public List<PlayerGearInventoryItemDto> Gear { get; set; } = new();
    }
}