using BackendApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Text.Json;

namespace BackendApi.Infrastructure.Persistence
{
    public sealed class GameDbSeeder
    {
        private static readonly Guid DefaultMerchantId =
            Guid.Parse("22222222-2222-2222-2222-222222222222");

        private readonly GameDbContext _dbContext;

        public GameDbSeeder(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            await _dbContext.Database.MigrateAsync();

            await EnsureDefaultMerchantAsync();
            await SeedCardDefinitionsAsync();
            await SeedGearSetDefinitionsAsync();
            await SeedGearDefinitionsAsync();
            await SeedMonstersAsync();

            await _dbContext.SaveChangesAsync();
        }

        private async Task EnsureDefaultMerchantAsync()
        {
            var merchant = await _dbContext.Merchants
                .FirstOrDefaultAsync(x => x.MerchantId == DefaultMerchantId);

            if (merchant is null)
            {
                merchant = new Merchant
                {
                    MerchantId = DefaultMerchantId,
                    Name = "Starter Merchant"
                };

                _dbContext.Merchants.Add(merchant);
            }
            else
            {
                merchant.Name = "Starter Merchant";
            }
        }

        private async Task SeedCardDefinitionsAsync()
        {
            var seedItems = ReadEmbeddedJson<List<CardSeedItem>>("cards.json");

            var existingByKey = await _dbContext.CardDefinitions
                .ToDictionaryAsync(x => x.Key);

            foreach (var item in seedItems)
            {
                if (!existingByKey.TryGetValue(item.Key, out var entity))
                {
                    entity = new CardDefinition
                    {
                        CardDefinitionId = Guid.NewGuid(),
                        Key = item.Key
                    };

                    _dbContext.CardDefinitions.Add(entity);
                    existingByKey[item.Key] = entity;
                }

                entity.Name = item.Name;
                entity.Description = item.Description;
                entity.ManaCost = item.ManaCost;
                entity.Price = item.Price;
                entity.EffectType = item.EffectType;
                entity.EffectValue = item.EffectValue;
                entity.Rarity = item.Rarity;
                entity.IconKey = item.IconKey;
                entity.IsActive = item.IsActive;
                entity.IsMerchantAvailable = item.IsMerchantAvailable;
            }
        }

        private async Task SeedGearSetDefinitionsAsync()
        {
            var seedItems = ReadEmbeddedJson<List<GearSetSeedItem>>("gear-sets.json");

            var existingBySetKey = await _dbContext.GearSetDefinitions
                .ToDictionaryAsync(x => x.SetKey);

            foreach (var item in seedItems)
            {
                if (!existingBySetKey.TryGetValue(item.SetKey, out var entity))
                {
                    entity = new GearSetDefinition
                    {
                        GearSetDefinitionId = Guid.NewGuid(),
                        SetKey = item.SetKey
                    };

                    _dbContext.GearSetDefinitions.Add(entity);
                    existingBySetKey[item.SetKey] = entity;
                }

                entity.Name = item.Name;
                entity.ThreePieceBonusDescription = item.ThreePieceBonusDescription;
            }
        }

        private async Task SeedGearDefinitionsAsync()
        {
            var seedItems = ReadEmbeddedJson<List<GearSeedItem>>("gear.json");

            var existingByKey = await _dbContext.GearDefinitions
                .ToDictionaryAsync(x => x.Key);

            foreach (var item in seedItems)
            {
                if (!existingByKey.TryGetValue(item.Key, out var entity))
                {
                    entity = new GearDefinition
                    {
                        GearDefinitionId = Guid.NewGuid(),
                        Key = item.Key
                    };

                    _dbContext.GearDefinitions.Add(entity);
                    existingByKey[item.Key] = entity;
                }

                entity.Name = item.Name;
                entity.Description = item.Description;
                entity.Slot = item.Slot;
                entity.Rarity = item.Rarity;
                entity.Price = item.Price;
                entity.ArmorValue = item.ArmorValue;
                entity.SetKey = item.SetKey;
                entity.IconKey = item.IconKey;
                entity.IsMerchantAvailable = item.IsMerchantAvailable;
            }
        }

        private async Task SeedMonstersAsync()
        {
            var seedItems = ReadEmbeddedJson<List<MonsterSeedModel>>("monsters.json");

            var existingByKey = await _dbContext.MonsterDefinitions
                .ToDictionaryAsync(x => x.MonsterKey);

            foreach (var item in seedItems)
            {
                if (!existingByKey.TryGetValue(item.MonsterKey, out var entity))
                {
                    entity = new MonsterDefinition
                    {
                        MonsterDefinitionId = Guid.NewGuid(),
                        MonsterKey = item.MonsterKey
                    };

                    _dbContext.MonsterDefinitions.Add(entity);
                    existingByKey[item.MonsterKey] = entity;
                }

                entity.Name = item.Name;
                entity.MaxHealth = item.MaxHealth;
                entity.Damage = item.Damage;
                entity.Mana = item.Mana;
                entity.GoldReward = item.GoldReward;
                entity.ExperienceReward = item.ExperienceReward;
            }
        }

        private static T ReadEmbeddedJson<T>(string fileName)
        {
            var assembly = typeof(GameDbSeeder).Assembly;

            var resourceName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith($".SeedData.{fileName}", StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                throw new InvalidOperationException(
                    $"Embedded resource for '{fileName}' was not found.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new InvalidOperationException(
                    $"Embedded resource stream for '{fileName}' was not found.");
            }

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            var result = JsonSerializer.Deserialize<T>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize embedded JSON file '{fileName}'.");
            }

            return result;
        }

        private sealed class CardSeedItem
        {
            public string Key { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int ManaCost { get; set; }
            public int Price { get; set; }
            public string EffectType { get; set; } = string.Empty;
            public int EffectValue { get; set; }
            public string Rarity { get; set; } = string.Empty;
            public string IconKey { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public bool IsMerchantAvailable { get; set; }
        }

        private sealed class GearSetSeedItem
        {
            public string SetKey { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? ThreePieceBonusDescription { get; set; }
        }

        private sealed class GearSeedItem
        {
            public string Key { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Slot { get; set; } = string.Empty;
            public string Rarity { get; set; } = string.Empty;
            public int Price { get; set; }
            public int ArmorValue { get; set; }
            public string? SetKey { get; set; }
            public string IconKey { get; set; } = string.Empty;
            public bool IsMerchantAvailable { get; set; }
        }

        private sealed class MonsterSeedModel
        {
            public string MonsterKey { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int MaxHealth { get; set; }
            public int Damage { get; set; }
            public int Mana { get; set; }
            public int GoldReward { get; set; }
            public int ExperienceReward { get; set; }
        }
    }
}