using System;
using Game.Infrastructure.Api.Dtos.Merchant;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Merchant.Views
{
    public sealed class MerchantCardOfferItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardNameText = null!;
        [SerializeField] private TMP_Text rarityText = null!;
        [SerializeField] private TMP_Text manaCostText = null!;
        [SerializeField] private TMP_Text effectText = null!;
        [SerializeField] private TMP_Text priceText = null!;
        [SerializeField] private Button buyButton = null!;
        [SerializeField] private TMP_Text buyButtonText = null!;

        public void Bind(MerchantOfferResponseDto offer, Action<MerchantOfferResponseDto> onBuyClicked)
        {
            cardNameText.text = offer.cardName;
            rarityText.text = offer.rarity;
            manaCostText.text = $"ManaCost: {offer.manaCost}";
            effectText.text = $"{offer.effectType}: {offer.effectValue}";
            priceText.text = $"Price: {offer.price}";
            buyButtonText.text = offer.isSold ? "Sold" : "Buy";
            buyButton.interactable = !offer.isSold;

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuyClicked(offer));
        }
    }
}