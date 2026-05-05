using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Inventory;
using Game.Infrastructure.Api.Inventory;
using Game.Infrastructure.Auth;
using Game.Presentation.Inventory.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Game.Presentation.Inventory.Controllers
{
    public sealed class InventoryPanelScript : MonoBehaviour
    {
        private const string StashLocation = "Stash";
        private const string LoadoutLocation = "Loadout";

        [Header("API")]
        [SerializeField] private string baseUrl = "https://localhost:7038";

        [Header("Mode")]
        [SerializeField] private bool showLoadoutSections = true;
        [SerializeField] private bool showStashSections = true;

        [Header("Tabs")]
        [SerializeField] private Button cardsTabButton = null!;
        [SerializeField] private Button gearTabButton = null!;
        [SerializeField] private GameObject cardsRoot = null!;
        [SerializeField] private GameObject gearRoot = null!;

        [Header("Cards Sections")]
        [SerializeField] private GameObject loadoutCardsSection = null!;
        [SerializeField] private Transform loadoutCardsContentParent = null!;
        [SerializeField] private GameObject stashCardsSection = null!;
        [SerializeField] private Transform stashCardsContentParent = null!;

        [Header("Gear Sections")]
        [SerializeField] private GameObject equippedGearSection = null!;
        [SerializeField] private Transform equippedGearContentParent = null!;
        [SerializeField] private GameObject stashGearSection = null!;
        [SerializeField] private Transform stashGearContentParent = null!;

        [Header("Prefabs")]
        [SerializeField] private InventoryCardItemView cardItemPrefab = null!;
        [SerializeField] private InventoryGearItemView gearItemPrefab = null!;

        [Header("Status")]
        [SerializeField] private TMP_Text statusText = null!;
        [SerializeField] private TMP_Text cardLoadoutCountText = null!;

        private PlayerInventoryApiClient _inventoryApiClient = null!;
        private bool _isBusy;

        private void Awake()
        {
            var authTokenStore = new AuthTokenStore();
            _inventoryApiClient = new PlayerInventoryApiClient(baseUrl, authTokenStore);

            cardsTabButton.onClick.AddListener(ShowCardsTab);
            gearTabButton.onClick.AddListener(ShowGearTab);

            ShowCardsTab();
        }

        public async Task LoadInventoryAsync()
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Loading inventory...");

                var inventory = await _inventoryApiClient.GetInventoryAsync();
                RenderInventory(inventory);

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                Debug.LogError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RenderInventory(PlayerInventoryResponseDto inventory)
        {
            RenderCards(inventory.cards);
            RenderGear(inventory.gear);
        }

        private void RenderCards(List<PlayerCardInventoryItemDto> cards)
        {
            ClearChildren(loadoutCardsContentParent);
            ClearChildren(stashCardsContentParent);

            var loadoutCards = cards
                .Where(x => x.location == LoadoutLocation)
                .OrderBy(x => x.loadoutOrder)
                .ToList();

            var stashCards = cards
                .Where(x => x.location == StashLocation)
                .OrderBy(x => x.cardName)
                .ToList();

            SetActiveIfAssigned(loadoutCardsSection, showLoadoutSections);
            SetActiveIfAssigned(stashCardsSection, showStashSections);

            if (cardLoadoutCountText != null)
            {
                cardLoadoutCountText.text = $"Loadout Cards: {loadoutCards.Count}/10";
            }

            if (showLoadoutSections && loadoutCardsContentParent != null)
            {
                foreach (var card in loadoutCards)
                {
                    var item = Instantiate(cardItemPrefab, loadoutCardsContentParent);
                    item.Bind(card, true, OnStashCardClicked);
                }
            }

            if (showStashSections && stashCardsContentParent != null)
            {
                foreach (var card in stashCards)
                {
                    var item = Instantiate(cardItemPrefab, stashCardsContentParent);
                    item.Bind(card, false, OnEquipCardClicked);
                }
            }
        }

        private void RenderGear(List<PlayerGearInventoryItemDto> gear)
        {
            ClearChildren(equippedGearContentParent);
            ClearChildren(stashGearContentParent);

            var equippedGear = gear
                .Where(x => x.location == LoadoutLocation)
                .OrderBy(x => GetGearSlotSortOrder(x.slot))
                .ToList();

            var stashGear = gear
                .Where(x => x.location == StashLocation)
                .OrderBy(x => GetGearSlotSortOrder(x.slot))
                .ThenBy(x => x.gearName)
                .ToList();

            SetActiveIfAssigned(equippedGearSection, showLoadoutSections);
            SetActiveIfAssigned(stashGearSection, showStashSections);

            if (showLoadoutSections && equippedGearContentParent != null)
            {
                foreach (var gearItem in equippedGear)
                {
                    var item = Instantiate(gearItemPrefab, equippedGearContentParent);
                    item.Bind(gearItem, true, OnStashGearClicked);
                }
            }

            if (showStashSections && stashGearContentParent != null)
            {
                foreach (var gearItem in stashGear)
                {
                    var item = Instantiate(gearItemPrefab, stashGearContentParent);
                    item.Bind(gearItem, false, OnEquipGearClicked);
                }
            }
        }

        private async void OnEquipCardClicked(PlayerCardInventoryItemDto card)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Equipping card...");

                var inventory = await _inventoryApiClient.EquipCardAsync(card.playerCardId);
                RenderInventory(inventory);

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                Debug.LogError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnStashCardClicked(PlayerCardInventoryItemDto card)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Moving card to stash...");

                var inventory = await _inventoryApiClient.StashCardAsync(card.playerCardId);
                RenderInventory(inventory);

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                Debug.LogError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnEquipGearClicked(PlayerGearInventoryItemDto gear)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Equipping gear...");

                var inventory = await _inventoryApiClient.EquipGearAsync(gear.playerGearId);
                RenderInventory(inventory);

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                Debug.LogError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void OnStashGearClicked(PlayerGearInventoryItemDto gear)
        {
            if (_isBusy)
            {
                return;
            }

            try
            {
                SetBusy(true);
                SetStatus("Moving gear to stash...");

                var inventory = await _inventoryApiClient.StashGearAsync(gear.playerGearId);
                RenderInventory(inventory);

                SetStatus(string.Empty);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                Debug.LogError(ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ShowCardsTab()
        {
            cardsRoot.SetActive(true);
            gearRoot.SetActive(false);
        }

        private void ShowGearTab()
        {
            cardsRoot.SetActive(false);
            gearRoot.SetActive(true);
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            cardsTabButton.interactable = !isBusy;
            gearTabButton.interactable = !isBusy;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static int GetGearSlotSortOrder(string slot)
        {
            return slot switch
            {
                "Helmet" => 1,
                "Chest" => 2,
                "Legs" => 3,
                _ => 99
            };
        }

        private static void SetActiveIfAssigned(GameObject gameObject, bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActive(active);
            }
        }
        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}