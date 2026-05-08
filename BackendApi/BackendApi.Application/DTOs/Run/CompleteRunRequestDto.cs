using System;

namespace BackendApi.Application.DTOs.Run
{
    public sealed class CompleteRunRequestDto
    {
        public int GoldEarned { get; set; }
        public int ExperienceEarned { get; set; }

        // Allowed values:
        // "Defeat"
        // "ReturnedHome"
        public string Outcome { get; set; } = string.Empty;

        public Guid MerchantId { get; set; }
    }
}