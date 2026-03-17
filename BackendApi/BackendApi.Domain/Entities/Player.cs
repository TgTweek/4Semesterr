using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendApi.Domain.Entities
{
    public class Player
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Level { get; set; } = 0;
        public int DaluMoney { get; set; } = 0;







    }
}
