using System;
using System.Collections.Generic;

namespace BackendApi.Domain.Entities
{
    public sealed class Player
    {
        public Guid PlayerId { get; set; }
        public Guid AppUserId { get; set; }
        public string PlayerName { get; set; } = string.Empty;

        public int Level { get; set; } = 1;
        public int DaluMoney { get; set; } = 200;
        public int Experience { get; set; } = 0;
        public int DamageBonus { get; set; } = 0;
        public int BaseMaxHealth { get; set; } = 30;
        public int BaseMaxMana { get; set; } = 3;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<PlayerCard> OwnedCards { get; set; } = new List<PlayerCard>();
        public ICollection<PlayerGear> OwnedGear { get; set; } = new List<PlayerGear>();

    }
}