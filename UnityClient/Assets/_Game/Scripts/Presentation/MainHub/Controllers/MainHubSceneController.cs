using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.MainHub.Controllers
{
    public sealed class MainHubSceneController : MonoBehaviour
    {
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

        [Header("Close Buttons")]
        [SerializeField] private Button merchantCloseButton = null!;
        [SerializeField] private Button houseCloseButton = null!;
        [SerializeField] private Button chestCloseButton = null!;
        [SerializeField] private Button worldMapCloseButton = null!;

        [Header("Actions")]
        [SerializeField] private Button startRunButton = null!;

        private void Awake()
        {
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

        private void OpenMerchantPanel()
        {
            ShowOnlyPanel(merchantPanel);
        }

        private void OpenHousePanel()
        {
            ShowOnlyPanel(housePanel);
        }

        private void OpenChestPanel()
        {
            ShowOnlyPanel(chestPanel);
        }

        private void OpenWorldMapPanel()
        {
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
            Debug.Log("Start Run clicked. World/run flow is not implemented yet.");
        }
    }
}