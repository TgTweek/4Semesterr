using System;
using System.Collections.Generic;

namespace Game.Infrastructure.Api.Dtos.Inventory
{
    [Serializable]
    public sealed class PlayerInventoryResponseDto
    {
        public List<PlayerCardInventoryItemDto> cards = new();
        public List<PlayerGearInventoryItemDto> gear = new();
    }
}