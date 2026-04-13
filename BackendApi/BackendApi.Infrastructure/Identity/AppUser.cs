using Microsoft.AspNetCore.Identity;

namespace BackendApi.Infrastructure.Identity
{
    public sealed class AppUser : IdentityUser<Guid>
    {
        public BackendApi.Domain.Entities.Player? Player { get; set; }
    }
}