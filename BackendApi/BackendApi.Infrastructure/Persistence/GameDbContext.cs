using BackendApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.PlayerId);

            entity.Property(p => p.PlayerName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Level)
                .IsRequired();

            entity.Property(p => p.DaluMoney)
                .IsRequired();

           
        });
    }
}