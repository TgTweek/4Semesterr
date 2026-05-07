using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Game.Infrastructure.Auth;
using Game.Presentation.Merchant.Controllers;
using Game.Presentation.Inventory.Controllers;

namespace Game.Presentation.MainHub.Controllers
{
    public sealed class MainHubSceneController : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string boardSceneName = "MainGameBoard";
        [SerializeField] private string loginSceneName = "Login";

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

        private AuthTokenStore _authTokenStore = null!;
        private bool _isBusy;

        private void Awake()
        {
            _authTokenStore = new AuthTokenStore();

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

            CloseAllPanels();
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
            SceneManager.LoadScene(boardSceneName);
        }
    }
}