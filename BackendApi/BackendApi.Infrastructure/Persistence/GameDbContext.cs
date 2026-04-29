using BackendApi.Domain.Entities;
using BackendApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Persistence
{
    public class GameDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<MonsterDefinition> MonsterDefinitions => Set<MonsterDefinition>();
        public DbSet<CardDefinition> CardDefinitions => Set<CardDefinition>();
        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<PlayerCard> PlayerCards => Set<PlayerCard>();

        public DbSet<GearDefinition> GearDefinitions => Set<GearDefinition>();
        public DbSet<GearSetDefinition> GearSetDefinitions => Set<GearSetDefinition>();
        public DbSet<PlayerGear> PlayerGears => Set<PlayerGear>();
        public DbSet<PlayerMerchantCardOffer> PlayerMerchantCardOffers => Set<PlayerMerchantCardOffer>();
        public DbSet<PlayerMerchantGearOffer> PlayerMerchantGearOffers => Set<PlayerMerchantGearOffer>();

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
                    .IsRequired()
                    .HasDefaultValue(1);

                entity.Property(x => x.DaluMoney)
                    .IsRequired()
                    .HasDefaultValue(200);

                entity.Property(x => x.Experience)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(x => x.DamageBonus)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(x => x.BaseMaxHealth)
                    .IsRequired()
                    .HasDefaultValue(30);

                entity.Property(x => x.BaseMaxMana)
                    .IsRequired()
                    .HasDefaultValue(3);

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

            modelBuilder.Entity<MonsterDefinition>(entity =>
            {
                entity.HasKey(x => x.MonsterDefinitionId);

                entity.Property(x => x.MonsterKey)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.MonsterKey)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.MaxHealth)
                    .IsRequired();

                entity.Property(x => x.Damage)
                    .IsRequired();

                entity.Property(x => x.Mana)
                    .IsRequired();

                entity.Property(x => x.GoldReward)
                    .IsRequired();

                entity.Property(x => x.ExperienceReward)
                    .IsRequired();
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

                entity.Property(x => x.IsMerchantAvailable)
                    .IsRequired();
            });

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.HasKey(x => x.MerchantId);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);
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

            modelBuilder.Entity<GearDefinition>(entity =>
            {
                entity.HasKey(x => x.GearDefinitionId);

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

                entity.Property(x => x.Slot)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Rarity)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Price)
                    .IsRequired();

                entity.Property(x => x.ArmorValue)
                    .IsRequired();

                entity.Property(x => x.SetKey)
                    .HasMaxLength(100);

                entity.Property(x => x.IconKey)
                    .HasMaxLength(100);

                entity.Property(x => x.IsMerchantAvailable)
                    .IsRequired();
            });

            modelBuilder.Entity<GearSetDefinition>(entity =>
            {
                entity.HasKey(x => x.GearSetDefinitionId);

                entity.Property(x => x.SetKey)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.SetKey)
                    .IsUnique();

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.ThreePieceBonusDescription)
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<PlayerGear>(entity =>
            {
                entity.HasKey(x => x.PlayerGearId);

                entity.HasOne(x => x.Player)
                    .WithMany()
                    .HasForeignKey(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.GearDefinition)
                    .WithMany()
                    .HasForeignKey(x => x.GearDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.AcquiredAtUtc)
                    .IsRequired();
            });

            modelBuilder.Entity<PlayerMerchantCardOffer>(entity =>
            {
                entity.HasKey(x => x.PlayerMerchantCardOfferId);

                entity.HasOne(x => x.Player)
                    .WithMany()
                    .HasForeignKey(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Merchant)
                    .WithMany()
                    .HasForeignKey(x => x.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CardDefinition)
                    .WithMany()
                    .HasForeignKey(x => x.CardDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.Price)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.IsSold)
                    .IsRequired();

                entity.Property(x => x.GeneratedAtUtc)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => new { x.PlayerId, x.MerchantId, x.DisplayOrder })
                    .IsUnique();
            });

            modelBuilder.Entity<PlayerMerchantGearOffer>(entity =>
            {
                entity.HasKey(x => x.PlayerMerchantGearOfferId);

                entity.HasOne(x => x.Player)
                    .WithMany()
                    .HasForeignKey(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Merchant)
                    .WithMany()
                    .HasForeignKey(x => x.MerchantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.GearDefinition)
                    .WithMany()
                    .HasForeignKey(x => x.GearDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.Price)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.IsSold)
                    .IsRequired();

                entity.Property(x => x.GeneratedAtUtc)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRowVersion();

                entity.HasIndex(x => new { x.PlayerId, x.MerchantId, x.DisplayOrder })
                    .IsUnique();
            });
        }
    }
}