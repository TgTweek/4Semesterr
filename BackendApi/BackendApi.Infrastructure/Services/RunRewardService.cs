using BackendApi.Application.DTOs.Run;
using BackendApi.Application.Interfaces;
using BackendApi.Domain.Entities;
using BackendApi.Domain.Services;
using BackendApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Infrastructure.Services
{
    public sealed class RunRewardService : IRunRewardService
    {
        private const string OutcomeDefeat = "Defeat";
        private const string OutcomeReturnedHome = "ReturnedHome";
        private const string OutcomeBossVictory = "BossVictory";
        private const string StarterStrikeKey = "starter_strike";
        private const string StarterBlockKey = "starter_block";

        private readonly GameDbContext _dbContext;
        private readonly IMerchantService _merchantService;

        public RunRewardService(
            GameDbContext dbContext,
            IMerchantService merchantService)
        {
            _dbContext = dbContext;
            _merchantService = merchantService;
        }

        public async Task<CompleteRunResponseDto> CompleteRunAsync(Guid appUserId, CompleteRunRequestDto request)
        {
            ValidateRequest(request);

            var outcome = NormalizeOutcome(request.Outcome);
            var isDefeat = outcome == OutcomeDefeat;
            var isBossVictory = outcome == OutcomeBossVictory;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var player = await _dbContext.Players
                .FirstOrDefaultAsync(x => x.AppUserId == appUserId);

            if (player is null)
            {
                throw new KeyNotFoundException("Player not found.");
            }

            ApplyRewards(player, request.GoldEarned, request.ExperienceEarned);

            var lostCardsCount = 0;

            if (isDefeat)
            {
                lostCardsCount = await RemoveNonStarterLoadoutCardsAsync(player.PlayerId);
                await NormalizeStarterLoadoutOrderAsync(player.PlayerId);
            }
            if (isBossVictory)
            {
                player.BossesDefeated++;
                player.DifficultyTier++;

                if (player.DifficultyTier > player.HighestDifficultyTierReached)
                {
                    player.HighestDifficultyTierReached = player.DifficultyTier;
                }
            }

            await _dbContext.SaveChangesAsync();

            await _merchantService.RefreshInventoryAsync(appUserId, request.MerchantId);

            await transaction.CommitAsync();

            return new CompleteRunResponseDto
            {
                Outcome = outcome,

                GoldEarned = request.GoldEarned,
                ExperienceEarned = request.ExperienceEarned,

                TotalGold = player.DaluMoney,

                NewLevel = player.Level,
                MaxLevel = PlayerProgressionRules.MaxLevel,
                TotalExperience = player.Experience,
                ExperienceRequiredForNextLevel = PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level),

                DamageBonus = PlayerProgressionRules.GetDamageBonus(player.Level),
                BaseMaxHealth = PlayerProgressionRules.GetBaseMaxHealth(player.Level),
                BaseMaxMana = PlayerProgressionRules.GetBaseMaxMana(player.Level),
                MovementTilesPerTurn = PlayerProgressionRules.GetMovementTilesPerTurn(player.Level),
                DifficultyTier = player.DifficultyTier,
                HighestDifficultyTierReached = player.HighestDifficultyTierReached,
                BossesDefeated = player.BossesDefeated,
                DifficultyIncreased = isBossVictory,
                LostCardsCount = lostCardsCount,
                ShopRefreshed = true,

                Message = isDefeat
                  ? $"Defeat completed. Lost {lostCardsCount} non-starter loadout cards."
                  : isBossVictory
                     ? $"Boss defeated. Difficulty increased to tier {player.DifficultyTier}."
                              : "Run completed. Returned home safely."
            };
        }

        private async Task<int> RemoveNonStarterLoadoutCardsAsync(Guid playerId)
        {
            var cardsToLose = await _dbContext.PlayerCards
                .Include(x => x.CardDefinition)
                .Where(x =>
                    x.PlayerId == playerId &&
                    x.Location == InventoryItemLocation.Loadout &&
                    x.CardDefinition.Key != StarterStrikeKey &&
                    x.CardDefinition.Key != StarterBlockKey)
                .ToListAsync();

            if (cardsToLose.Count == 0)
            {
                return 0;
            }

            _dbContext.PlayerCards.RemoveRange(cardsToLose);

            return cardsToLose.Count;
        }

        private async Task NormalizeStarterLoadoutOrderAsync(Guid playerId)
        {
            var remainingStarterCards = await _dbContext.PlayerCards
                .Include(x => x.CardDefinition)
                .Where(x =>
                    x.PlayerId == playerId &&
                    x.Location == InventoryItemLocation.Loadout &&
                    (
                        x.CardDefinition.Key == StarterStrikeKey ||
                        x.CardDefinition.Key == StarterBlockKey
                    ))
                .OrderBy(x => x.LoadoutOrder ?? int.MaxValue)
                .ThenBy(x => x.AcquiredAtUtc)
                .ToListAsync();

            var loadoutOrder = 1;

            foreach (var card in remainingStarterCards)
            {
                card.LoadoutOrder = loadoutOrder;
                loadoutOrder++;
            }
        }

        private static void ValidateRequest(CompleteRunRequestDto request)
        {
            if (request.GoldEarned < 0)
            {
                throw new InvalidOperationException("GoldEarned cannot be negative.");
            }

            if (request.ExperienceEarned < 0)
            {
                throw new InvalidOperationException("ExperienceEarned cannot be negative.");
            }

            if (request.MerchantId == Guid.Empty)
            {
                throw new InvalidOperationException("MerchantId is required.");
            }

            _ = NormalizeOutcome(request.Outcome);
        }

        private static string NormalizeOutcome(string outcome)
        {
            if (string.IsNullOrWhiteSpace(outcome))
            {
                throw new InvalidOperationException("Outcome is required.");
            }

            var normalized = outcome.Trim();

            if (string.Equals(normalized, OutcomeDefeat, StringComparison.OrdinalIgnoreCase))
            {
                return OutcomeDefeat;
            }

            if (string.Equals(normalized, OutcomeBossVictory, StringComparison.OrdinalIgnoreCase))
            {
                return OutcomeBossVictory;
            }

            if (string.Equals(normalized, OutcomeReturnedHome, StringComparison.OrdinalIgnoreCase))
            {
                return OutcomeReturnedHome;
            }

            throw new InvalidOperationException("Outcome must be 'Defeat', 'ReturnedHome', or 'BossVictory'.");
        }

        private static void ApplyRewards(Player player, int goldEarned, int experienceEarned)
        {
            player.DaluMoney += goldEarned;

            if (player.Level >= PlayerProgressionRules.MaxLevel)
            {
                player.Level = PlayerProgressionRules.MaxLevel;
                player.Experience = 0;
                return;
            }

            player.Experience += experienceEarned;

            while (player.Level < PlayerProgressionRules.MaxLevel &&
                   player.Experience >= PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level))
            {
                var requiredXp = PlayerProgressionRules.GetRequiredExperienceForNextLevel(player.Level);
                player.Experience -= requiredXp;
                player.Level++;
            }

            if (player.Level >= PlayerProgressionRules.MaxLevel)
            {
                player.Level = PlayerProgressionRules.MaxLevel;
                player.Experience = 0;
            }
        }
    }
}