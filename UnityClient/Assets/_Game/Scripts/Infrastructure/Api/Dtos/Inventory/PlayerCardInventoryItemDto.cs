using System;

namespace Game.Infrastructure.Api.Dtos.Inventory
{
    [Serializable]
    public sealed class PlayerCardInventoryItemDto
    {
        public string playerCardId = "";
        public string cardDefinitionId = "";

        public string cardKey = "";
        public string cardName = "";
        public string description = "";

        public int manaCost;
        public string effectType = "";
        public int effectValue;

        public string rarity = "";
        public string iconKey = "";

        public string location = "";
        public int loadoutOrder;
    }
}