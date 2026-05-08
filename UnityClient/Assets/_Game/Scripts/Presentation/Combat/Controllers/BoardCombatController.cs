using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Application.Combat.Commands;
using Game.Application.Combat.UseCases;
using Game.Domain.Entities;
using Game.Domain.ValueObjects;
using Game.Infrastructure.Api.Combat;
using Game.Infrastructure.Api.Dtos.Combat;
using Game.Infrastructure.Api.Dtos.Inventory;
using Game.Infrastructure.Api.Inventory;
using Game.Infrastructure.Api.Player;
using Game.Infrastructure.Auth;
using Game.Presentation.Combat.Views;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Presentation.Combat.Controllers
{
    public sealed class BoardCombatController : MonoBehaviour
    {
        [Serializable]
        private sealed class MonsterSpriteEntry
        {
            public string monsterKey = string.Empty;
            public Sprite sprite = null!;
        }

        [Header("API")]
        [SerializeField] private string baseUrl = "https://localhost:7038";

        [Header("Scene references")]
        [SerializeField] private Camera worldCamera = null!;
        [SerializeField] private Grid grid = null!;
        [SerializeField] private Transform playerTransform = null!;

        [Header("Monsters")]
        [SerializeField] private Transform spawnedMonstersRoot = null!;
        [SerializeField] private MonsterView monsterPrefab = null!;
        [SerializeField] private List<MonsterSpawnPoint> spawnPoints = new();
        [SerializeField, Range(2, 5)] private int minMonsterCount = 2;
        [SerializeField, Range(2, 5)] private int maxMonsterCount = 5;
        [SerializeField] private List<MonsterSpriteEntry> monsterSprites = new();

        [Header("Spell bar")]
        [SerializeField] private Transform spellSlotsRoot = null!;
        [SerializeField] private CombatSpellSlotView spellSlotPrefab = null!;
        [SerializeField] private Button cancelSelectedSpellButton = null!;

        [Header("UI")]
        [SerializeField] private TMP_Text playerStatsText = null!;
        [SerializeField] private TMP_Text pendingRewardsText = null!;
        [SerializeField] private TMP_Text turnText = null!;
        [SerializeField] private TMP_Text battleLogText = null!;

        [Header("Enemy turn timing")]
        [SerializeField] private float enemyTurnDelaySeconds = 0.35f;

        private AuthTokenStore _authTokenStore = null!;
        private PlayerApiGateway _playerApiGateway = null!;
        private MonsterApiGateway _monsterApiGateway = null!;
        private PlayerInventoryApiClient _playerInventoryApiClient = null!;

        private CheckTargetInRangeUseCase _checkTargetInRangeUseCase = null!;
        private CastSpellAtMonsterUseCase _castSpellAtMonsterUseCase = null!;
        private UseSelfSpellUseCase _useSelfSpellUseCase = null!;
        private MonsterAttackUseCase _monsterAttackUseCase = null!;

        private BoardCombatState _combatState = null!;

        private readonly List<MonsterView> _spawnedMonsterViews = new();
        private readonly List<CombatSpellSlotView> _spawnedSpellSlots = new();

        private PlayerCardInventoryItemDto _selectedDamageCard = null!;
        private bool _hasSelectedDamageCard;

        public bool CanUseMovementInput()
        {
            return _combatState != null &&
                   !_combatState.IsFinished &&
                   _combatState.IsPlayerTurn &&
                   !_hasSelectedDamageCard;
        }

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();

            _playerApiGateway = new PlayerApiGateway(baseUrl, _authTokenStore);
            _monsterApiGateway = new MonsterApiGateway(baseUrl);
            _playerInventoryApiClient = new PlayerInventoryApiClient(baseUrl, _authTokenStore);

            _checkTargetInRangeUseCase = new CheckTargetInRangeUseCase();
            _castSpellAtMonsterUseCase = new CastSpellAtMonsterUseCase(_checkTargetInRangeUseCase);
            _useSelfSpellUseCase = new UseSelfSpellUseCase();
            _monsterAttackUseCase = new MonsterAttackUseCase(_checkTargetInRangeUseCase);

            if (cancelSelectedSpellButton != null)
            {
                cancelSelectedSpellButton.onClick.RemoveAllListeners();
                cancelSelectedSpellButton.onClick.AddListener(CancelSelectedSpell);
            }
        }

        private async void Start()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
            await InitializeCombatAsync();
        }

        private async Task InitializeCombatAsync()
        {
            ClearSpawnedMonsters();
            ClearSpellSlots();
            CancelSelectedSpellSilently();

            if (grid == null || playerTransform == null)
            {
                Debug.LogError("BoardCombatController is missing Grid or Player Transform.");
                SetLog("Combat setup error: Grid or Player Transform missing.");
                return;
            }

            if (spawnedMonstersRoot == null || monsterPrefab == null)
            {
                Debug.LogError("BoardCombatController is missing SpawnedMonstersRoot or MonsterPrefab.");
                SetLog("Combat setup error: Monster root or prefab missing.");
                return;
            }

            if (spawnPoints.Count == 0)
            {
                Debug.LogError("BoardCombatController has no spawn points.");
                SetLog("Combat setup error: No spawn points configured.");
                return;
            }

            if (spellSlotsRoot == null || spellSlotPrefab == null)
            {
                Debug.LogError("BoardCombatController is missing SpellSlotsRoot or SpellSlotPrefab.");
                SetLog("Combat setup error: Spell bar not configured.");
                return;
            }

            if (!_authTokenStore.HasAccessToken())
            {
                SetLog("No access token found. Login first.");
                return;
            }

            try
            {
                SetLog("Fetching player...");
                var playerDto = await _playerApiGateway.GetCurrentPlayerAsync();

                SetLog("Fetching monsters...");
                var monsterCatalog = await _monsterApiGateway.GetAllMonstersAsync();

                var playerState = new PlayerRuntimeState(
                    playerDto.level,
                    playerDto.baseMaxHealth,
                    playerDto.baseMaxMana,
                    playerDto.damageBonus,
                    playerDto.movementTilesPerTurn);

                SetLog("Spawning monsters...");
                var monsterStates = SpawnRandomMonstersFromCatalog(monsterCatalog);

                _combatState = new BoardCombatState(playerState, monsterStates);
                _combatState.StartPlayerTurn();

                var loadoutCards = await TryLoadLoadoutCardsAsync();
                if (loadoutCards.Count > 0)
                {
                    BuildSpellSlots(loadoutCards);
                    _combatState.SetLog("Combat ready.");
                }
                else
                {
                    _combatState.SetLog("Combat ready. No loadout cards available.");
                }

                RefreshAllUi();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                SetLog(ex.Message);
            }
        }

        private async Task<List<PlayerCardInventoryItemDto>> TryLoadLoadoutCardsAsync()
        {
            try
            {
                SetLog("Fetching inventory...");
                var inventoryDto = await _playerInventoryApiClient.GetInventoryAsync();

                var loadoutCards = inventoryDto.cards
                    .Where(x => IsLoadoutLocation(x.location))
                    .OrderBy(x => x.loadoutOrder)
                    .ToList();

                Debug.Log($"Inventory loaded. Loadout cards found: {loadoutCards.Count}");
                return loadoutCards;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Inventory could not be loaded for combat: {ex.Message}");
                return new List<PlayerCardInventoryItemDto>();
            }
        }

        private static bool IsLoadoutLocation(string location)
        {
            return string.Equals(location?.Trim(), "Loadout", StringComparison.OrdinalIgnoreCase);
        }

        private List<MonsterRuntimeState> SpawnRandomMonstersFromCatalog(List<MonsterDto> monsterCatalog)
        {
            if (monsterCatalog == null || monsterCatalog.Count == 0)
            {
                throw new InvalidOperationException("No monsters were returned from backend.");
            }

            var availableSpawnPoints = new List<MonsterSpawnPoint>(spawnPoints);
            availableSpawnPoints.RemoveAll(x => x == null);

            if (availableSpawnPoints.Count == 0)
            {
                throw new InvalidOperationException("No valid spawn points found.");
            }

            var result = new List<MonsterRuntimeState>();

            var safeMin = Mathf.Clamp(minMonsterCount, 2, 5);
            var safeMax = Mathf.Clamp(maxMonsterCount, safeMin, 5);

            var randomCount = UnityEngine.Random.Range(safeMin, safeMax + 1);
            var finalCount = Mathf.Min(randomCount, availableSpawnPoints.Count);

            for (var i = 0; i < finalCount; i++)
            {
                var spawnPoint = PickAndRemoveRandom(availableSpawnPoints);
                var monsterDto = PickRandom(monsterCatalog);

                var runtimeState = new MonsterRuntimeState(
                    monsterDto.monsterKey,
                    monsterDto.name,
                    monsterDto.maxHealth,
                    monsterDto.damage,
                    monsterDto.mana,
                    monsterDto.goldReward,
                    monsterDto.experienceReward);

                var monsterView = Instantiate(
                    monsterPrefab,
                    spawnPoint.transform.position,
                    Quaternion.identity,
                    spawnedMonstersRoot);

                monsterView.name = $"{monsterDto.name}_{i + 1}";
                monsterView.BindState(runtimeState);

                var sprite = ResolveMonsterSprite(monsterDto.monsterKey);
                if (sprite != null)
                {
                    monsterView.SetSprite(sprite);
                }
                else
                {
                    Debug.LogWarning($"No sprite mapping found for monster key '{monsterDto.monsterKey}'.");
                }

                _spawnedMonsterViews.Add(monsterView);
                result.Add(runtimeState);
            }

            Debug.Log($"Spawned monsters: {result.Count}");
            return result;
        }

        private void BuildSpellSlots(List<PlayerCardInventoryItemDto> loadoutCards)
        {
            foreach (var card in loadoutCards)
            {
                var slot = Instantiate(spellSlotPrefab, spellSlotsRoot);
                slot.Bind(card, OnSpellSlotClicked);
                _spawnedSpellSlots.Add(slot);
            }
        }

        private void ClearSpellSlots()
        {
            foreach (var slot in _spawnedSpellSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            _spawnedSpellSlots.Clear();
        }

        private void OnSpellSlotClicked(PlayerCardInventoryItemDto card)
        {
            if (_combatState == null || _combatState.IsFinished || !_combatState.IsPlayerTurn)
            {
                return;
            }

            if (string.Equals(card.effectType, "Damage", StringComparison.OrdinalIgnoreCase))
            {
                _selectedDamageCard = card;
                _hasSelectedDamageCard = true;
                _combatState.SetLog($"{card.cardName} selected. Click a monster.");
                RefreshAllUi();
                return;
            }

            if (string.Equals(card.effectType, "Block", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(card.effectType, "Heal", StringComparison.OrdinalIgnoreCase))
            {
                CancelSelectedSpellSilently();

                _useSelfSpellUseCase.Execute(
                    _combatState,
                    card.cardName,
                    card.manaCost,
                    card.effectType,
                    card.effectValue);

                RefreshAllUi();
                return;
            }

            _combatState.SetLog($"Unsupported effect type: {card.effectType}");
            RefreshAllUi();
        }

        public void CancelSelectedSpell()
        {
            if (!_hasSelectedDamageCard)
            {
                return;
            }

            CancelSelectedSpellSilently();

            if (_combatState != null)
            {
                _combatState.SetLog("Selected spell cancelled.");
            }

            RefreshAllUi();
        }

        private void CancelSelectedSpellSilently()
        {
            _selectedDamageCard = null!;
            _hasSelectedDamageCard = false;
        }
        
        public bool TryConsumeMovement(int movementCost)
        {
            if (_combatState == null) return false;
            if (!_combatState.IsPlayerTurn) return false;

            var ok = _combatState.Player.TrySpendMovement(movementCost);
            if (!ok)
            {
                _combatState.SetLog("Not enough movement tiles left.");
            }

            RefreshAllUi();
            return ok;
        }

        public void EndPlayerTurn()
        {
            if (_combatState == null) return;
            if (_combatState.IsFinished) return;
            if (!_combatState.IsPlayerTurn) return;

            CancelSelectedSpellSilently();
            StartCoroutine(RunEnemyRoundCoroutine());
        }

        private IEnumerator RunEnemyRoundCoroutine()
        {
            if (_combatState == null || grid == null || playerTransform == null)
            {
                yield break;
            }

            _combatState.EndPlayerTurn();
            RefreshAllUi();

            var playerCell = ToCellPosition(grid.WorldToCell(playerTransform.position));

            foreach (var monsterView in _spawnedMonsterViews)
            {
                if (_combatState.IsFinished)
                {
                    break;
                }

                var monsterState = monsterView.RuntimeState;
                if (monsterState == null || monsterState.IsDead)
                {
                    continue;
                }

                var monsterCell = ToCellPosition(grid.WorldToCell(monsterView.transform.position));

                var log = _monsterAttackUseCase.Execute(
                    _combatState.Player,
                    monsterState,
                    monsterCell,
                    playerCell);

                _combatState.SetLog(log);
                RefreshAllUi();

                yield return new WaitForSeconds(enemyTurnDelaySeconds);
            }

            if (!_combatState.IsFinished)
            {
                _combatState.StartPlayerTurn();
                RefreshAllUi();
            }
        }

        private void Update()
        {
            if (_combatState == null) return;
            if (_combatState.IsFinished) return;
            if (!_combatState.IsPlayerTurn) return;
            if (!_hasSelectedDamageCard) return;
            if (worldCamera == null) return;
            if (grid == null) return;
            if (Mouse.current == null) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelSelectedSpell();
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelSelectedSpell();
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            var screenPosition = Mouse.current.position.ReadValue();
            var worldPoint = worldCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    Mathf.Abs(worldCamera.transform.position.z)));

            var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (!hit.collider)
            {
                return;
            }

            var monsterView = hit.collider.GetComponent<MonsterView>();
            if (monsterView == null)
            {
                monsterView = hit.collider.GetComponentInParent<MonsterView>();
            }

            if (monsterView == null)
            {
                return;
            }

            var monsterState = monsterView.RuntimeState;
            if (monsterState == null || monsterState.IsDead)
            {
                return;
            }

            var command = CreateDamageCommand(_selectedDamageCard);
            var playerCell = ToCellPosition(grid.WorldToCell(playerTransform.position));
            var monsterCell = ToCellPosition(grid.WorldToCell(monsterView.transform.position));

            var success = _castSpellAtMonsterUseCase.Execute(
                _combatState,
                monsterState,
                playerCell,
                monsterCell,
                command);

            monsterView.Refresh();

            if (success)
            {
                CancelSelectedSpellSilently();
            }

            RefreshAllUi();
        }

        private CastSpellAtMonsterCommand CreateDamageCommand(PlayerCardInventoryItemDto card)
        {
            return new CastSpellAtMonsterCommand
            {
                SpellName = card.cardName,
                ManaCost = card.manaCost,
                EffectValue = card.effectValue,
                Range = GetDamageRange(card)
            };
        }

        private int GetDamageRange(PlayerCardInventoryItemDto card)
        {
            if (!string.Equals(card.effectType, "Damage", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            var lowerName = card.cardName.ToLowerInvariant();

            if (lowerName.Contains("fireball") ||
                lowerName.Contains("poison") ||
                lowerName.Contains("ember") ||
                lowerName.Contains("frost"))
            {
                return 3;
            }

            return 1;
        }

        private Sprite ResolveMonsterSprite(string monsterKey)
        {
            foreach (var entry in monsterSprites)
            {
                if (entry != null &&
                    string.Equals(entry.monsterKey, monsterKey, StringComparison.OrdinalIgnoreCase) &&
                    entry.sprite != null)
                {
                    return entry.sprite;
                }
            }

            return null!;
        }

        private static T PickRandom<T>(List<T> items)
        {
            var index = UnityEngine.Random.Range(0, items.Count);
            return items[index];
        }

        private static T PickAndRemoveRandom<T>(List<T> items)
        {
            var index = UnityEngine.Random.Range(0, items.Count);
            var item = items[index];
            items.RemoveAt(index);
            return item;
        }

        private void ClearSpawnedMonsters()
        {
            foreach (var monsterView in _spawnedMonsterViews)
            {
                if (monsterView != null)
                {
                    Destroy(monsterView.gameObject);
                }
            }

            _spawnedMonsterViews.Clear();
        }

        private void RefreshAllUi()
        {
            if (_combatState == null)
            {
                return;
            }

            foreach (var monsterView in _spawnedMonsterViews)
            {
                monsterView.Refresh();
            }

            foreach (var slot in _spawnedSpellSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                var isSelected = _hasSelectedDamageCard &&
                                 slot.PlayerCardId == _selectedDamageCard.playerCardId;

                slot.SetSelected(isSelected);
                slot.SetInteractable(_combatState.IsPlayerTurn && !_combatState.IsFinished);
            }

            if (playerStatsText != null)
            {
                playerStatsText.text =
                    $"HP {_combatState.Player.CurrentHealth}/{_combatState.Player.MaxHealth}\n" +
                    $"Mana {_combatState.Player.CurrentMana}/{_combatState.Player.MaxMana}\n" +
                    $"Block {_combatState.Player.Block}\n" +
                    $"Move {_combatState.Player.RemainingMovementTiles}/{_combatState.Player.MovementTilesPerTurn}";
            }

            if (pendingRewardsText != null)
            {
                pendingRewardsText.text =
                    $"Pending Gold: {_combatState.PendingRunGold}\n" +
                    $"Pending XP: {_combatState.PendingRunExperience}";
            }

            if (turnText != null)
            {
                turnText.text = _combatState.IsFinished
                    ? (_combatState.Player.IsDead ? "Defeat" : "Victory")
                    : (_combatState.IsPlayerTurn ? "Player Turn" : "Enemy Turn");
            }

            if (battleLogText != null)
            {
                battleLogText.text = _combatState.LastLog;
            }
        }

        private void SetLog(string text)
        {
            if (_combatState != null)
            {
                _combatState.SetLog(text);
                RefreshAllUi();
                return;
            }

            if (battleLogText != null)
            {
                battleLogText.text = text;
            }

            Debug.Log(text);
        }

        private static CellPosition ToCellPosition(Vector3Int cell)
        {
            return new CellPosition(cell.x, cell.y);
        }
    }
}