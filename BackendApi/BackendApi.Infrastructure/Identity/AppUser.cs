using System;
using BackendApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BackendApi.Infrastructure.Identity
{
    public sealed class AppUser : IdentityUser<Guid>
    {
        public Player? Player { get; set; }
    }
}