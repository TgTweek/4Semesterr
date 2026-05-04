using System;

namespace Game.Infrastructure.Api.Dtos.Inventory
{
    [Serializable]
    public sealed class PlayerGearInventoryItemDto
    {
        public string playerGearId = "";
        public string gearDefinitionId = "";

        public string gearKey = "";
        public string gearName = "";
        public string description = "";

        public string slot = "";
        public string rarity = "";

        public int armorValue;

        public string setKey = "";
        public string setName = "";
        public string threePieceBonusDescription = "";

        public string iconKey = "";

        public string location = "";
    }
}