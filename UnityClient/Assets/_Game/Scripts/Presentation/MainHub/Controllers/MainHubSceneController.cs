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

        private AuthTokenStore _authTokenStore = null!;
        private RunSessionStore _runSessionStore = null!;
        private PlayerApiGateway _playerApiGateway = null!;
        private bool _isBusy;

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();
            _runSessionStore = new RunSessionStore();

            _playerApiGateway = new PlayerApiGateway(apiBaseUrl, _authTokenStore);
            merchantHotspotButton.onClick.RemoveAllListeners();
            houseHotspotButton.onClick.RemoveAllListeners();
            chestHotspotButton.onClick.RemoveAllListeners();
            worldMapHotspotButton.onClick.RemoveAllListeners();

            merchantCloseButton.onClick.RemoveAllListeners();
            houseCloseButton.onClick.RemoveAllListeners();
            chestCloseButton.onClick.RemoveAllListeners();
            worldMapCloseButton.onClick.RemoveAllListeners();

            startRunButton.onClick.RemoveAllListeners();

            merchantHotspotButton.onClick.AddListener(OpenMerchantPanel);
            houseHotspotButton.onClick.AddListener(OpenHousePanel);
            chestHotspotButton.onClick.AddListener(OpenChestPanel);
            worldMapHotspotButton.onClick.AddListener(OpenWorldMapPanel);

            merchantCloseButton.onClick.AddListener(CloseAllPanels);
            houseCloseButton.onClick.AddListener(CloseAllPanels);
            chestCloseButton.onClick.AddListener(CloseAllPanels);
            worldMapCloseButton.onClick.AddListener(CloseAllPanels);

            startRunButton.onClick.AddListener(OnStartRunClicked);

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
            if (_isBusy) return;

            ShowOnlyPanel(merchantPanel);
            await merchantPanelScript.LoadInventoryAsync();
        }

        private async void OpenHousePanel()
        {
            if (_isBusy) return;

            ShowOnlyPanel(housePanel);
            await houseInventoryPanelScript.LoadInventoryAsync();
        }

        private async void OpenChestPanel()
        {
            if (_isBusy) return;

            ShowOnlyPanel(chestPanel);
            await chestInventoryPanelScript.LoadInventoryAsync();
        }

        private void OpenWorldMapPanel()
        {
            if (_isBusy) return;

            ShowOnlyPanel(worldMapPanel);
        }

        private void ShowOnlyPanel(GameObject panelToShow)
        {
            dimOverlay.SetActive(true);

            merchantPanel.SetActive(false);
            housePanel.SetActive(false);
            chestPanel.SetActive(false);
            worldMapPanel.SetActive(false);

            panelToShow.SetActive(true);
        }

        private void CloseAllPanels()
        {
            dimOverlay.SetActive(false);

            merchantPanel.SetActive(false);
            housePanel.SetActive(false);
            chestPanel.SetActive(false);
            worldMapPanel.SetActive(false);
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
            goldText.text = "Gold: --";
            levelText.text = "Level: --";
            xpText.text = "XP: -- / --";

            if (xpFillImage != null)
            {
                xpFillImage.fillAmount = 0f;
            }
        }

        private void ApplyPlayerHud(PlayerDto player)
        {
            goldText.text = $"Gold: {player.daluMoney}";
            levelText.text = $"Level: {player.level}";

            if (player.maxLevel > 0 && player.level >= player.maxLevel)
            {
                xpText.text = "XP: MAX";
                xpFillImage.fillAmount = 1f;
                return;
            }

            var required = Mathf.Max(1, player.experienceRequiredForNextLevel);
            var current = Mathf.Clamp(player.experience, 0, required);

            xpText.text = $"XP: {current} / {required}";
            xpFillImage.fillAmount = Mathf.Clamp01((float)current / required);
        }
    }
}