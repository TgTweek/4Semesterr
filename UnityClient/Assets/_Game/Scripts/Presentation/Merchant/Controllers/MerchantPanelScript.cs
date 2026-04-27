using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Infrastructure.Api.Dtos.Merchant;
using Game.Infrastructure.Api.Merchant;
using Game.Infrastructure.Auth;
using Game.Presentation.Merchant.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Merchant.Controllers
{
    public sealed class MerchantPanelScript : MonoBehaviour
    {
        private const string DefaultMerchantId = "22222222-2222-2222-2222-222222222222";

        [Header("API")]
        [SerializeField] private string baseUrl = "https://localhost:7038";
        [SerializeField] private string merchantId = DefaultMerchantId;

        [Header("Tabs")]
        [SerializeField] private Button cardsTabButton = null!;
        [SerializeField] private Button gearTabButton = null!;
        [SerializeField] private GameObject cardsRoot = null!;
        [SerializeField] private GameObject gearRoot = null!;

        [Header("Content Parents")]
        [SerializeField] private Transform cardsContentParent = null!;
        [SerializeField] private Transform gearContentParent = null!;

        [Header("Prefabs")]
        [SerializeField] private MerchantCardOfferItemView merchantCardOfferItemPrefab = null!;
        [SerializeField] private MerchantGearOfferItemView merchantGearOfferItemPrefab = null!;

        [Header("Status")]
        [SerializeField] private TMP_Text errorText = null!;

        private MerchantApiClient _merchantApiClient = null!;

        private void Awake()
        {
            var authTokenStore = new AuthTokenStore();
            _merchantApiClient = new MerchantApiClient(baseUrl, authTokenStore);

            cardsTabButton.onClick.AddListener(ShowCardsTab);
            gearTabButton.onClick.AddListener(ShowGearTab);
        }

        public async Task LoadInventoryAsync()
        {
            try
            {
                errorText.text = string.Empty;

                var inventory = await _merchantApiClient.GetInventoryAsync(merchantId);

                RenderCards(inventory.cardOffers);
                RenderGear(inventory.gearOffers);

                ShowCardsTab();
            }
            catch (Exception ex)
            {
                errorText.text = ex.Message;
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

        private void RenderCards(List<MerchantOfferResponseDto> offers)
        {
            ClearChildren(cardsContentParent);

            foreach (var offer in offers)
            {
                var item = Instantiate(merchantCardOfferItemPrefab, cardsContentParent);
                item.Bind(offer, OnBuyCardClicked);
            }
        }

        private void RenderGear(List<MerchantGearOfferResponseDto> offers)
        {
            ClearChildren(gearContentParent);

            foreach (var offer in offers)
            {
                var item = Instantiate(merchantGearOfferItemPrefab, gearContentParent);
                item.Bind(offer, OnBuyGearClicked);
            }
        }

        private async void OnBuyCardClicked(MerchantOfferResponseDto offer)
        {
            try
            {
                errorText.text = string.Empty;

                var result = await _merchantApiClient.BuyCardAsync(merchantId, offer.offerId);

                if (!result.success)
                {
                    errorText.text = result.message;
                    return;
                }

                await LoadInventoryAsync();
            }
            catch (Exception ex)
            {
                errorText.text = ex.Message;
            }
        }

        private async void OnBuyGearClicked(MerchantGearOfferResponseDto offer)
        {
            try
            {
                errorText.text = string.Empty;

                var result = await _merchantApiClient.BuyGearAsync(merchantId, offer.offerId);

                if (!result.success)
                {
                    errorText.text = result.message;
                    return;
                }

                await LoadInventoryAsync();
            }
            catch (Exception ex)
            {
                errorText.text = ex.Message;
            }
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}