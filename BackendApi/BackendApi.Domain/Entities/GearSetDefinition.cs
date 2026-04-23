using System;

namespace BackendApi.Domain.Entities
{
    public sealed class GearSetDefinition
    {
        public Guid GearSetDefinitionId { get; set; }

        public string SetKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

      
        public string? ThreePieceBonusDescription { get; set; }
    }
}