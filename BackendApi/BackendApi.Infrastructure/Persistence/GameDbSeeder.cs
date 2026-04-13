using BackendApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Persistence
{
    public sealed class GameDbSeeder
    {
        private readonly GameDbContext _dbContext;

        public GameDbSeeder(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            await _dbContext.Database.MigrateAsync();

            

            if (!await _dbContext.CardDefinitions.AnyAsync())
            {
                var fireball = new CardDefinition
                {
                    CardDefinitionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Key = "fireball",
                    Name = "Fireball",
                    Description = "Deal 10 damage.",
                    ManaCost = 2,
                    Price = 25,
                    EffectType = "Damage",
                    EffectValue = 10,
                    Rarity = "Common",
                    IconKey = "fireball",
                    IsActive = true
                };

                var shieldBash = new CardDefinition
                {
                    CardDefinitionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Key = "shield_bash",
                    Name = "Shield Bash",
                    Description = "Deal 6 damage.",
                    ManaCost = 1,
                    Price = 20,
                    EffectType = "Damage",
                    EffectValue = 6,
                    Rarity = "Common",
                    IconKey = "shield_bash",
                    IsActive = true
                };

                var quickStrike = new CardDefinition
                {
                    CardDefinitionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Key = "quick_strike",
                    Name = "Quick Strike",
                    Description = "Deal 4 damage.",
                    ManaCost = 1,
                    Price = 15,
                    EffectType = "Damage",
                    EffectValue = 4,
                    Rarity = "Common",
                    IconKey = "quick_strike",
                    IsActive = true
                };

                _dbContext.CardDefinitions.AddRange(fireball, shieldBash, quickStrike);
            }

            if (!await _dbContext.Merchants.AnyAsync())
            {
                var merchant = new Merchant
                {
                    MerchantId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Starter Merchant",
                    IsActive = true
                };

                _dbContext.Merchants.Add(merchant);
            }

            await _dbContext.SaveChangesAsync();

            if (!await _dbContext.MerchantOffers.AnyAsync())
            {
                var merchantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

                var offers = new[]
                {
                    new MerchantOffer
                    {
                        MerchantOfferId = Guid.Parse("33333333-3333-3333-3333-333333333331"),
                        MerchantId = merchantId,
                        CardDefinitionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        Price = 25,
                        Stock = 10,
                        IsActive = true
                    },
                    new MerchantOffer
                    {
                        MerchantOfferId = Guid.Parse("33333333-3333-3333-3333-333333333332"),
                        MerchantId = merchantId,
                        CardDefinitionId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        Price = 20,
                        Stock = 10,
                        IsActive = true
                    },
                    new MerchantOffer
                    {
                        MerchantOfferId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        MerchantId = merchantId,
                        CardDefinitionId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                        Price = 15,
                        Stock = 10,
                        IsActive = true
                    }
                };

                _dbContext.MerchantOffers.AddRange(offers);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}