using BackendApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BackendApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BackendApi.Infrastructure.Persistence
{
    public class GameDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<CardDefinition> CardDefinitions => Set<CardDefinition>();
        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<MerchantOffer> MerchantOffers => Set<MerchantOffer>();
        public DbSet<PlayerCard> PlayerCards => Set<PlayerCard>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(x => x.PlayerId);

                entity.Property(x => x.PlayerName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Level)
                    .IsRequired();

                entity.Property(x => x.DaluMoney)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => x.AppUserId)
                    .IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasOne(x => x.Player)
                    .WithOne()
                    .HasForeignKey<Player>(x => x.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<CardDefinition>(entity =>
            {
                entity.HasKey(x => x.CardDefinitionId);

                entity.Property(x => x.Key)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.Key)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.Property(x => x.EffectType)
                    .HasMaxLength(100);

                entity.Property(x => x.Rarity)
                    .HasMaxLength(50);

                entity.Property(x => x.IconKey)
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(x => x.MerchantId);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<MerchantOffer>(entity =>
            {
                entity.HasKey(x => x.MerchantOfferId);

                entity.HasOne(x => x.Merchant)
                    .WithMany(x => x.Offers)
                    .HasForeignKey(x => x.MerchantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CardDefinition)
                    .WithMany()
                    .HasForeignKey(x => x.CardDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.Price)
                    .IsRequired();

                entity.Property(x => x.Stock)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();
            });

            modelBuilder.Entity<PlayerCard>(entity =>
            {
                entity.HasKey(x => x.PlayerCardId);

                entity.HasOne(x => x.Player)
                    .WithMany(x => x.OwnedCards)
                    .HasForeignKey(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CardDefinition)
                    .WithMany()
                    .HasForeignKey(x => x.CardDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.AcquiredAtUtc)
                    .IsRequired();
            });
        }
    }
}