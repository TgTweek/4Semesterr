using System;
using System.Collections.Generic;

namespace BackendApi.Domain.Entities
{
    public sealed class Player
    {
        public Guid PlayerId { get; set; }

        public Guid AppUserId { get; set; }

        public string PlayerName { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public int DaluMoney { get; set; } = 0;

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<PlayerCard> OwnedCards { get; set; } = new List<PlayerCard>();
    }
}