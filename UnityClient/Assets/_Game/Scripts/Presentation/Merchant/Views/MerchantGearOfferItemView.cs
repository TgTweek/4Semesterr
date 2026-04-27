using System;
using Game.Infrastructure.Api.Dtos.Merchant;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Merchant.Views
{
    public sealed class MerchantGearOfferItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text gearNameText = null!;
        [SerializeField] private TMP_Text slotText = null!;
        [SerializeField] private TMP_Text rarityText = null!;
        [SerializeField] private TMP_Text armorText = null!;
        [SerializeField] private TMP_Text setText = null!;
        [SerializeField] private TMP_Text priceText = null!;
        [SerializeField] private Button buyButton = null!;
        [SerializeField] private TMP_Text buyButtonText = null!;

        public void Bind(MerchantGearOfferResponseDto offer, Action<MerchantGearOfferResponseDto> onBuyClicked)
        {
            gearNameText.text = offer.gearName;
            slotText.text = offer.slot;
            rarityText.text = offer.rarity;
            armorText.text = $"Armor: {offer.armorValue}";
            setText.text = string.IsNullOrWhiteSpace(offer.setName)
                ? string.Empty
                : $"{offer.setName} - {offer.threePieceBonusDescription}";
            priceText.text = $"{offer.price}";
            buyButtonText.text = offer.isSold ? "Sold" : "Buy";
            buyButton.interactable = !offer.isSold;

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuyClicked(offer));
        }
    }
}