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
using Game.Infrastructure.Run;
using Game.Presentation.Combat.Views;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using UnityEngine.EventSystems;

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

        [Header("Scenes")]
        [SerializeField] private string inGameMapSceneName = "InGameMap";
        [SerializeField] private float combatResultDelaySeconds = 0.8f;

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
        private RunSessionStore _runSessionStore = null!;

        private PlayerApiGateway _playerApiGateway = null!;
        private MonsterApiGateway _monsterApiGateway = null!;
        private PlayerInventoryApiClient _playerInventoryApiClient = null!;

        private CheckTargetInRangeUseCase _checkTargetInRangeUseCase = null!;
        private CastSpellAtMonsterUseCase _castSpellAtMonsterUseCase = null!;
        private UseSelfSpellUseCase _useSelfSpellUseCase = null!;
        private MonsterAttackUseCase _monsterAttackUseCase = null!;
        private MonsterAdvanceUseCase _monsterAdvanceUseCase = null!;

        private BoardCombatState _combatState = null!;

        private readonly List<MonsterView> _spawnedMonsterViews = new();
        private readonly List<CombatSpellSlotView> _spawnedSpellSlots = new();

        private PlayerCardInventoryItemDto _selectedDamageCard = null!;
        private bool _hasSelectedDamageCard;
        private bool _combatResolved;
        private MonsterView _hoveredMonsterView = null!;
        public bool CanUseMovementInput()
        {
            return _combatState != null &&
                   !_combatState.IsFinished &&
                   _combatState.IsPlayerTurn &&
                   !_hasSelectedDamageCard;
        }
        public IReadOnlyList<CellPosition> GetOccupiedMonsterCells()
        {
            var result = new List<CellPosition>();

            if (grid == null)
            {
                return result;
            }

            foreach (var monsterView in _spawnedMonsterViews)
            {
                if (monsterView == null)
                {
                    continue;
                }

                var monsterState = monsterView.RuntimeState;
                if (monsterState == null || monsterState.IsDead)
                {
                    continue;
                }

                var monsterCell = grid.WorldToCell(monsterView.transform.position);
                result.Add(ToCellPosition(monsterCell));
            }

            return result;
        }
        public bool IsMonsterOccupyingCell(Vector3Int cell)
        {
            if (grid == null)
            {
                return false;
            }

            foreach (var monsterView in _spawnedMonsterViews)
            {
                if (monsterView == null)
                {
                    continue;
                }

                var monsterState = monsterView.RuntimeState;
                if (monsterState == null || monsterState.IsDead)
                {
                    continue;
                }

                var monsterCell = grid.WorldToCell(monsterView.transform.position);
                if (monsterCell == cell)
                {
                    return true;
                }
            }

            return false;
        }
        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();
            _runSessionStore = new RunSessionStore();

            _playerApiGateway = new PlayerApiGateway(baseUrl, _authTokenStore);
            _monsterApiGateway = new MonsterApiGateway(baseUrl);
            _playerInventoryApiClient = new PlayerInventoryApiClient(baseUrl, _authTokenStore);

            _checkTargetInRangeUseCase = new CheckTargetInRangeUseCase();
            _castSpellAtMonsterUseCase = new CastSpellAtMonsterUseCase(_checkTargetInRangeUseCase);
            _useSelfSpellUseCase = new UseSelfSpellUseCase();
            _monsterAttackUseCase = new MonsterAttackUseCase(_checkTargetInRangeUseCase);
            _monsterAdvanceUseCase = new MonsterAdvanceUseCase();

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

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private async Task InitializeCombatAsync()
        {
            _combatResolved = false;

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

                var spawnCell = grid.WorldToCell(spawnPoint.transform.position);
                var spawnWorldPosition = grid.GetCellCenterWorld(spawnCell);

                var monsterView = Instantiate(
                    monsterPrefab,
                    spawnWorldPosition,
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
                slot.Bind(card, OnSpellSlotClicked, _combatState.Player.DamageBonus);
                _spawnedSpellSlots.Add(slot);
            }
        }

        private void ClearSpellSlots()
        {
            for (var i = _spawnedSpellSlots.Count - 1; i >= 0; i--)
            {
                var slot = _spawnedSpellSlots[i];

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
                TryResolveFinishedCombat();
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

            for (var i = 0; i < _spawnedMonsterViews.Count; i++)
            {
                var monsterView = _spawnedMonsterViews[i];

                if (monsterView == null)
                {
                    continue;
                }

                if (_combatState.IsFinished)
                {
                    break;
                }

                var monsterState = monsterView.RuntimeState;
                if (monsterState == null || monsterState.IsDead)
                {
                    continue;
                }

                var playerCell = ToCellPosition(grid.WorldToCell(playerTransform.position));
                var startCell = ToCellPosition(grid.WorldToCell(monsterView.transform.position));

                if (_checkTargetInRangeUseCase.Execute(startCell, playerCell, 1))
                {
                    var attackLog = _monsterAttackUseCase.Execute(
                        _combatState.Player,
                        monsterState,
                        startCell,
                        playerCell);

                    _combatState.SetLog(attackLog);
                    RefreshAllUi();

                    if (TryResolveFinishedCombat())
                    {
                        yield break;
                    }

                    yield return new WaitForSeconds(enemyTurnDelaySeconds);
                    continue;
                }

                var blockedCells = BuildBlockedCellsForMonster(monsterView, playerCell);
                var movedCell = _monsterAdvanceUseCase.Execute(startCell, playerCell, 2, blockedCells);

                if (!movedCell.Equals(startCell))
                {
                    MoveMonsterToCell(monsterView, movedCell);
                    _combatState.SetLog($"{monsterState.Name} moved closer.");
                    RefreshAllUi();

                    yield return new WaitForSeconds(enemyTurnDelaySeconds);
                }

                playerCell = ToCellPosition(grid.WorldToCell(playerTransform.position));

                if (_checkTargetInRangeUseCase.Execute(movedCell, playerCell, 1))
                {
                    var attackLog = _monsterAttackUseCase.Execute(
                        _combatState.Player,
                        monsterState,
                        movedCell,
                        playerCell);

                    _combatState.SetLog(attackLog);
                    RefreshAllUi();

                    if (TryResolveFinishedCombat())
                    {
                        yield break;
                    }
                }
                else if (movedCell.Equals(startCell))
                {
                    _combatState.SetLog($"{monsterState.Name} could not move closer.");
                    RefreshAllUi();
                }

                yield return new WaitForSeconds(enemyTurnDelaySeconds);
            }

            if (TryResolveFinishedCombat())
            {
                yield break;
            }

            if (!_combatState.IsFinished)
            {
                _combatState.StartPlayerTurn();
                RefreshAllUi();
            }
        }

        private HashSet<CellPosition> BuildBlockedCellsForMonster(MonsterView movingMonsterView, CellPosition playerCell)
        {
            var blockedCells = new HashSet<CellPosition>
            {
                playerCell
            };

            foreach (var otherMonsterView in _spawnedMonsterViews)
            {
                if (otherMonsterView == null || otherMonsterView == movingMonsterView)
                {
                    continue;
                }

                var otherState = otherMonsterView.RuntimeState;
                if (otherState == null || otherState.IsDead)
                {
                    continue;
                }

                var otherCell = ToCellPosition(grid.WorldToCell(otherMonsterView.transform.position));
                blockedCells.Add(otherCell);
            }

            return blockedCells;
        }

        private void MoveMonsterToCell(MonsterView monsterView, CellPosition targetCell)
        {
            if (monsterView == null)
            {
                return;
            }

            var targetWorld = grid.GetCellCenterWorld(new Vector3Int(targetCell.X, targetCell.Y, 0));
            monsterView.transform.position = targetWorld;
        }

        private void Update()
        {
            UpdateHoveredMonster();

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

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (!TryGetMonsterViewUnderMouse(out var monsterView))
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
            TryResolveFinishedCombat();
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
        private void UpdateHoveredMonster()
        {
            if (_combatState == null || _combatState.IsFinished)
            {
                SetHoveredMonster(null);
                return;
            }

            if (worldCamera == null || Mouse.current == null)
            {
                SetHoveredMonster(null);
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                SetHoveredMonster(null);
                return;
            }

            if (!TryGetMonsterViewUnderMouse(out var monsterView))
            {
                SetHoveredMonster(null);
                return;
            }

            SetHoveredMonster(monsterView);
        }

        private bool TryGetMonsterViewUnderMouse(out MonsterView monsterView)
        {
            monsterView = null!;

            if (worldCamera == null || Mouse.current == null)
            {
                return false;
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
                return false;
            }

            monsterView = hit.collider.GetComponent<MonsterView>();
            if (monsterView == null)
            {
                monsterView = hit.collider.GetComponentInParent<MonsterView>();
            }

            if (monsterView == null)
            {
                return false;
            }

            var monsterState = monsterView.RuntimeState;
            if (monsterState == null || monsterState.IsDead)
            {
                monsterView = null!;
                return false;
            }

            return true;
        }

        private void SetHoveredMonster(MonsterView monsterView)
        {
            if (_hoveredMonsterView == monsterView)
            {
                return;
            }

            if (_hoveredMonsterView != null)
            {
                _hoveredMonsterView.SetHovered(false);
            }

            _hoveredMonsterView = monsterView;

            if (_hoveredMonsterView != null)
            {
                _hoveredMonsterView.SetHovered(true);
            }
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

        private bool TryResolveFinishedCombat()
        {
            if (_combatState == null)
            {
                return false;
            }

            if (_combatResolved)
            {
                return true;
            }

            if (!_combatState.IsFinished)
            {
                return false;
            }

            _combatResolved = true;
            CancelSelectedSpellSilently();
            SetHoveredMonster(null);

            var goldEarned = Math.Max(0, _combatState.PendingRunGold);
            var experienceEarned = Math.Max(0, _combatState.PendingRunExperience);

            if (_combatState.Player.IsDead)
            {
                _runSessionStore.RegisterDefeat(goldEarned, experienceEarned);
                _combatState.SetLog("Defeat. Returning to run result screen...");
            }
            else
            {
                _runSessionStore.RegisterVictory(goldEarned, experienceEarned);
                _combatState.SetLog("Victory. Opening battle map...");
            }

            RefreshAllUi();
            StartCoroutine(LoadInGameMapAfterDelayCoroutine());

            return true;
        }

        private IEnumerator LoadInGameMapAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(combatResultDelaySeconds);

            if (string.IsNullOrWhiteSpace(inGameMapSceneName))
            {
                Debug.LogError("InGameMap scene name is missing on BoardCombatController.");
                yield break;
            }

            SceneManager.LoadScene(inGameMapSceneName);
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
            SetHoveredMonster(null);

            for (var i = _spawnedMonsterViews.Count - 1; i >= 0; i--)
            {
                var monsterView = _spawnedMonsterViews[i];

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

            if (_hoveredMonsterView == null ||
                _hoveredMonsterView.RuntimeState == null ||
                _hoveredMonsterView.RuntimeState.IsDead)
            {
                SetHoveredMonster(null);
            }

            for (var i = _spawnedMonsterViews.Count - 1; i >= 0; i--)
            {
                var monsterView = _spawnedMonsterViews[i];

                if (monsterView == null)
                {
                    _spawnedMonsterViews.RemoveAt(i);
                    continue;
                }

                monsterView.Refresh();
            }

            for (var i = _spawnedSpellSlots.Count - 1; i >= 0; i--)
            {
                var slot = _spawnedSpellSlots[i];

                if (slot == null)
                {
                    _spawnedSpellSlots.RemoveAt(i);
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
                        $"Damage Bonus +{_combatState.Player.DamageBonus}\n" +
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