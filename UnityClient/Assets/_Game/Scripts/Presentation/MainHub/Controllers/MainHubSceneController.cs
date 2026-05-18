using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Infrastructure.Auth;
using Game.Infrastructure.Api.Player;
using Game.Infrastructure.Api.Dtos.Player;
using Game.Presentation.Inventory.Controllers;
using Game.Presentation.Merchant.Controllers;
using Game.Infrastructure.Run;
using Game.Infrastructure.Combat;
using Game.Presentation.MainHub.Views;

namespace Game.Presentation.MainHub.Controllers
{
    public sealed class MainHubSceneController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string inGameMapSceneName = "InGameMap";
        [SerializeField] private string loginSceneName = "Login";

        [Header("API")]
        [SerializeField] private string apiBaseUrl = "https://localhost:7038";

        [Header("Overlay")]
        [SerializeField] private GameObject dimOverlay = null!;

        [Header("Hotspots")]
        [SerializeField] private Button merchantHotspotButton = null!;
        [SerializeField] private Button houseHotspotButton = null!;
        [SerializeField] private Button chestHotspotButton = null!;
        [SerializeField] private Button worldMapHotspotButton = null!;

        [Header("Panels")]
        [SerializeField] private GameObject merchantPanel = null!;
        [SerializeField] private GameObject housePanel = null!;
        [SerializeField] private GameObject chestPanel = null!;
        [SerializeField] private GameObject worldMapPanel = null!;

        [Header("Panel Controllers")]
        [SerializeField] private MerchantPanelScript merchantPanelScript = null!;

        [Header("Close Buttons")]
        [SerializeField] private Button merchantCloseButton = null!;
        [SerializeField] private Button houseCloseButton = null!;
        [SerializeField] private Button chestCloseButton = null!;
        [SerializeField] private Button worldMapCloseButton = null!;

        [Header("Actions")]
        [SerializeField] private Button startRunButton = null!;

        [Header("Inventory Controllers")]
        [SerializeField] private InventoryPanelScript houseInventoryPanelScript = null!;
        [SerializeField] private InventoryPanelScript chestInventoryPanelScript = null!;

        [Header("Player HUD")]
        [SerializeField] private TMP_Text goldText = null!;
        [SerializeField] private TMP_Text levelText = null!;
        [SerializeField] private TMP_Text xpText = null!;
        [SerializeField] private Image xpFillImage = null!;

        [Header("World Map Tier UI")]
        [SerializeField] private TMP_Text worldTierSummaryText = null!;
        [SerializeField] private Transform worldTierRowsParent = null!;
        [SerializeField] private WorldTierRowView worldTierRowPrefab = null!;
        [SerializeField] private int futureTiersToPreview = 5;

        private AuthTokenStore _authTokenStore = null!;
        private RunSessionStore _runSessionStore = null!;
        private PlayerApiGateway _playerApiGateway = null!;
        private MonsterRuntimeFactory _monsterRuntimeFactory = null!;

        private bool _isBusy;

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();
            _runSessionStore = new RunSessionStore();

            _playerApiGateway = new PlayerApiGateway(apiBaseUrl, _authTokenStore);
            _monsterRuntimeFactory = new MonsterRuntimeFactory();

            RegisterButton(merchantHotspotButton, OpenMerchantPanel);
            RegisterButton(houseHotspotButton, OpenHousePanel);
            RegisterButton(chestHotspotButton, OpenChestPanel);
            RegisterButton(worldMapHotspotButton, OpenWorldMapPanel);

            RegisterButton(merchantCloseButton, CloseAllPanels);
            RegisterButton(houseCloseButton, CloseAllPanels);
            RegisterButton(chestCloseButton, CloseAllPanels);
            RegisterButton(worldMapCloseButton, CloseAllPanels);

            RegisterButton(startRunButton, OnStartRunClicked);

            if (merchantPanelScript != null)
            {
                merchantPanelScript.PurchaseCompleted += OnMerchantPurchaseCompleted;
            }

            SetDefaultPlayerHud();
            CloseAllPanels();
        }

        private async void Start()
        {
            await RefreshPlayerHudAsync();
        }

        private void OnDestroy()
        {
            if (merchantPanelScript != null)
            {
                merchantPanelScript.PurchaseCompleted -= OnMerchantPurchaseCompleted;
            }
        }

        private async void OpenMerchantPanel()
        {
            if (_isBusy)
            {
                return;
            }

            ShowOnlyPanel(merchantPanel);

            if (merchantPanelScript != null)
            {
                await merchantPanelScript.LoadInventoryAsync();
            }
        }

        private async void OpenHousePanel()
        {
            if (_isBusy)
            {
                return;
            }

            ShowOnlyPanel(housePanel);

            if (houseInventoryPanelScript != null)
            {
                await houseInventoryPanelScript.LoadInventoryAsync();
            }
        }

        private async void OpenChestPanel()
        {
            if (_isBusy)
            {
                return;
            }

            ShowOnlyPanel(chestPanel);

            if (chestInventoryPanelScript != null)
            {
                await chestInventoryPanelScript.LoadInventoryAsync();
            }
        }

        private async void OpenWorldMapPanel()
        {
            if (_isBusy)
            {
                return;
            }

            ShowOnlyPanel(worldMapPanel);
            await RenderWorldMapPanelAsync();
        }

        private void ShowOnlyPanel(GameObject panelToShow)
        {
            SetActive(dimOverlay, true);

            SetActive(merchantPanel, false);
            SetActive(housePanel, false);
            SetActive(chestPanel, false);
            SetActive(worldMapPanel, false);

            SetActive(panelToShow, true);
        }

        private void CloseAllPanels()
        {
            SetActive(dimOverlay, false);

            SetActive(merchantPanel, false);
            SetActive(housePanel, false);
            SetActive(chestPanel, false);
            SetActive(worldMapPanel, false);
        }

        private void OnStartRunClicked()
        {
            if (_isBusy)
            {
                return;
            }

            if (!_authTokenStore.HasAccessToken())
            {
                Debug.LogWarning("No access token found. Redirecting to login.");
                SceneManager.LoadScene(loginSceneName);
                return;
            }

            _isBusy = true;

            _runSessionStore.StartNewRun();

            SceneManager.LoadScene(inGameMapSceneName);
        }

        private async void OnMerchantPurchaseCompleted()
        {
            await RefreshPlayerHudAsync();
        }

        private async Task RefreshPlayerHudAsync()
        {
            if (!_authTokenStore.HasAccessToken())
            {
                SetDefaultPlayerHud();
                return;
            }

            try
            {
                var player = await _playerApiGateway.GetCurrentPlayerAsync();
                ApplyPlayerHud(player);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load player HUD: {ex.Message}");
                SetDefaultPlayerHud();
            }
        }

        private void SetDefaultPlayerHud()
        {
            if (goldText != null)
            {
                goldText.text = "Gold: --";
            }

            if (levelText != null)
            {
                levelText.text = "Level: --";
            }

            if (xpText != null)
            {
                xpText.text = "XP: -- / --";
            }

            if (xpFillImage != null)
            {
                xpFillImage.fillAmount = 0f;
            }
        }

        private void ApplyPlayerHud(PlayerDto player)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {player.daluMoney}";
            }

            if (levelText != null)
            {
                levelText.text = $"Level: {player.level}";
            }

            if (xpText == null)
            {
                return;
            }

            if (player.maxLevel > 0 && player.level >= player.maxLevel)
            {
                xpText.text = "XP: MAX";

                if (xpFillImage != null)
                {
                    xpFillImage.fillAmount = 1f;
                }

                return;
            }

            var required = Mathf.Max(1, player.experienceRequiredForNextLevel);
            var current = Mathf.Clamp(player.experience, 0, required);

            xpText.text = $"XP: {current} / {required}";

            if (xpFillImage != null)
            {
                xpFillImage.fillAmount = Mathf.Clamp01((float)current / required);
            }
        }

        private async Task RenderWorldMapPanelAsync()
        {
            if (!_authTokenStore.HasAccessToken())
            {
                SceneManager.LoadScene(loginSceneName);
                return;
            }

            try
            {
                var player = await _playerApiGateway.GetCurrentPlayerAsync();

                RenderWorldTierSummary(
                    player.difficultyTier,
                    player.highestDifficultyTierReached,
                    player.bossesDefeated);

                RenderWorldTierRows(
                    player.difficultyTier,
                    player.highestDifficultyTierReached);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to render world map tier UI: {ex.Message}");

                if (worldTierSummaryText != null)
                {
                    worldTierSummaryText.text = "Could not load world tier data.";
                }

                ClearWorldTierRows();
            }
        }

        private void RenderWorldTierSummary(
            int difficultyTier,
            int highestDifficultyTierReached,
            int bossesDefeated)
        {
            if (worldTierSummaryText == null)
            {
                return;
            }

            var safeCurrentTier = Math.Max(0, difficultyTier);
            var safeHighestTier = Math.Max(0, highestDifficultyTierReached);
            var safeBossesDefeated = Math.Max(0, bossesDefeated);

            worldTierSummaryText.text =
                $"Current World Tier: {safeCurrentTier + 1}\n" +
                $"Highest Tier Reached: {safeHighestTier + 1}\n" +
                $"Bosses Defeated: {safeBossesDefeated}";
        }

        private void RenderWorldTierRows(
            int difficultyTier,
            int highestDifficultyTierReached)
        {
            ClearWorldTierRows();

            if (worldTierRowsParent == null || worldTierRowPrefab == null)
            {
                Debug.LogWarning("World tier rows parent or row prefab is not assigned.");
                return;
            }

            var safeCurrentTier = Math.Max(0, difficultyTier);
            var safeHighestTier = Math.Max(0, highestDifficultyTierReached);

            var maxTierToShow = Math.Max(
                safeHighestTier + futureTiersToPreview,
                safeCurrentTier + futureTiersToPreview);

            for (var tier = maxTierToShow; tier >= 0; tier--)
            {
                var tierToBind = tier;

                var isUnlocked = tierToBind <= safeHighestTier;
                var isCurrent = tierToBind == safeCurrentTier;
                var canSelect = isUnlocked && !isCurrent && !_isBusy;

                var row = Instantiate(worldTierRowPrefab, worldTierRowsParent);
                row.gameObject.SetActive(true);

                var rowText = _monsterRuntimeFactory.BuildDifficultyRow(
                    tierToBind,
                    safeCurrentTier,
                    safeHighestTier);

               

                row.Bind(
                    rowText,
                    canSelect,
                    () => OnWorldTierClicked(tierToBind));
            }
        }

        private async void OnWorldTierClicked(int difficultyTier)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);

                var player = await _playerApiGateway.SetDifficultyTierAsync(difficultyTier);

                ApplyPlayerHud(player);

                SetBusy(false);

                RenderWorldTierSummary(
                    player.difficultyTier,
                    player.highestDifficultyTierReached,
                    player.bossesDefeated);

                RenderWorldTierRows(
                    player.difficultyTier,
                    player.highestDifficultyTierReached);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to change world tier: {ex.Message}");
                SetBusy(false);
            }
        }

        private void ClearWorldTierRows()
        {
            if (worldTierRowsParent == null)
            {
                return;
            }

            for (var i = worldTierRowsParent.childCount - 1; i >= 0; i--)
            {
                Destroy(worldTierRowsParent.GetChild(i).gameObject);
            }
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            SetButtonInteractable(merchantHotspotButton, !isBusy);
            SetButtonInteractable(houseHotspotButton, !isBusy);
            SetButtonInteractable(chestHotspotButton, !isBusy);
            SetButtonInteractable(worldMapHotspotButton, !isBusy);

            SetButtonInteractable(merchantCloseButton, !isBusy);
            SetButtonInteractable(houseCloseButton, !isBusy);
            SetButtonInteractable(chestCloseButton, !isBusy);
            SetButtonInteractable(worldMapCloseButton, !isBusy);

            SetButtonInteractable(startRunButton, !isBusy);
        }

        private static void RegisterButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void SetButtonInteractable(Button button, bool isInteractable)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = isInteractable;
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target == null)
            {
                return;
            }

            target.SetActive(isActive);
        }
    }
}